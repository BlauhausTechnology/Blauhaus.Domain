using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Extensions;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Extensions;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Client.Sync.Client
{

    //todo 
    // here is the problem. If you sync once and get the newest and oldest entities, and then change the sync comnmand parameters and sync again
    // you will never get any new entities even if you deserve them, because you already have the newst and oldest possible entity on the device.
    // the only way to solve this is to make the device lookup for modified times specific to a given combination of command parameters. That is, ignoring 
    // modified times... so we can hash the command maybe and use it as a lookup? or maybe make the command filters immutable? 
    // how to exclude the non-filter properties from the hash? How did user sync on Cheeteye solve this? I actually don't think it does....
    // So if I sync users with TotalRewards > 10. and then Sync for TotalRewards > 11, the client side queries will load the data for the second query
    // using entities from the first query, but when calculating OlderThan we must add a parameter to the query that is unique to the   TotalRewards > 11 part
    // of the query so that we basically redownload all the  overlapping data. No other way to solve this. 
    // how do we save this hash locally? Maybe we don't have to - we just ignore all the user-specific filters when getting the OlderThan value
    // ... no but then we will ALWAYS redownload everything

    //possibly filtering and syncing don't mix. we should use syncing for static classifications not filters...?
    //maybe sync command can have two types of property: a category (or combination of categories), which is static and used for sync, and
    //filters, which are ignored for the purpose of syncing (ie SyncState == OutOfSync)

        //TODO SEARCH AND SYNC ARE TWO DIFFERENT THINGS. Sync command parameters must be categorical and mutually exclusive. Search results are ignored by sync
        // ... however perhaps they could share some infrastructure?

    public class SyncClient<TModel, TDto, TSyncCommand> : ISyncClient<TModel, TSyncCommand> 
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand 
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IConnectivityService _connectivityService;
        private readonly ISyncClientRepository<TModel, TDto, TSyncCommand> _syncClientRepository;
        private readonly ICommandHandler<SyncResult<TModel>, TSyncCommand> _syncCommandHandler;

        private event EventHandler LoadNextBatchEvent;
        private event EventHandler LoadNewFromServerEvent;
        private event EventHandler LoadNewFromClientEvent;
        private event EventHandler ReloadFromClientEvent;
        private event EventHandler CancelEvent;

        private long OldestModelPublished => _publishedModels.Count == 0 ? 0 : _publishedModels.Values.Min();
        private long NewestModelPublished => _publishedModels.Count == 0 ? 0 : _publishedModels.Values.Max();
        private readonly Dictionary<Guid, long> _publishedModels = new Dictionary<Guid, long>();

        private long _numberOfModelsToPublish;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _token;

        public SyncClient(
            IAnalyticsService analyticsService, 
            IConnectivityService connectivityService,
            ISyncClientRepository<TModel, TDto, TSyncCommand> syncClientRepository,
            ICommandHandler<SyncResult<TModel>, TSyncCommand> syncCommandHandler)
        {
            _analyticsService = analyticsService;
            _connectivityService = connectivityService;
            _syncClientRepository = syncClientRepository;
            _syncCommandHandler = syncCommandHandler;
        }

        public IObservable<TModel> Connect(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler)
        {
            TraceStatus(SyncClientState.Starting, $"SyncClient connected. Required: {syncRequirement} (batch size {syncCommand.BatchSize})", syncStatusHandler);

            _numberOfModelsToPublish = syncCommand.BatchSize;
            syncStatusHandler.PublishedEntities = 0;

            return Observable.Create<TModel>(async observer =>
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _token = _cancellationTokenSource.Token;
                
                ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);

                TraceStatus(SyncClientState.Starting, $"Initializing sync. Local status {syncStatus}", syncStatusHandler);
                
                syncStatusHandler.IsConnected = _connectivityService.IsConnectedToInternet;
                syncStatusHandler.AllLocalEntities = syncStatus.AllLocalEntities;
                syncStatusHandler.SyncedLocalEntities = syncStatus.SyncedLocalEntities;

                async void HandleLoadNextBatch(object s, EventArgs e)
                    => await LoadNextBatchAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);
                
                async void HandleLoadNewFromClient(object s, EventArgs e)
                    => await LoadNewFromClientAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);
                
                async void HandleReloadFromClient(object s, EventArgs e)
                    => await ReloadFromClientAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);

                async void HandleLoadNewFromServer(object s, EventArgs e)
                    => await LoadNewFromServerAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);

                void HandleCancel(object s, EventArgs e)
                    => Cancel(syncStatusHandler, _token);

                var loadNextBatchSubscription = Observable.FromEventPattern(x => LoadNextBatchEvent += HandleLoadNextBatch, x => LoadNextBatchEvent -= HandleLoadNextBatch).Subscribe();
                var loadNewFromServerSubscription = Observable.FromEventPattern(x => LoadNewFromServerEvent += HandleLoadNewFromServer, x => LoadNewFromServerEvent -= HandleLoadNewFromServer).Subscribe();
                var loadNewFromClientSubscription = Observable.FromEventPattern(x => LoadNewFromClientEvent += HandleLoadNewFromClient, x => LoadNewFromClientEvent -= HandleLoadNewFromClient).Subscribe();
                var reloadFromClientSubscription = Observable.FromEventPattern(x => ReloadFromClientEvent += HandleReloadFromClient, x => ReloadFromClientEvent -= HandleReloadFromClient).Subscribe();
                var cancelSubscription = Observable.FromEventPattern(x => CancelEvent += HandleCancel, x => CancelEvent -= HandleCancel).Subscribe();

                if (syncStatus.SyncedLocalEntities == 0)
                {
                    TraceStatus(SyncClientState.DownloadingOld, "No local data, checking server...", syncStatusHandler);
                    syncCommand.NewerThan = null;
                    syncCommand.OlderThan = null;

                    await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, false, _token);

                    syncStatusHandler.State = SyncClientState.Completed;
                }

                else
                {
                    //first load and publish local models
                    TraceStatus(SyncClientState.LoadingLocal, "Loading data from local store", syncStatusHandler);
                    var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
                    PublishModelsIfRequired(localModels, observer, syncStatusHandler);

                    //then update any new or modified entities
                    TraceStatus(SyncClientState.DownloadingNew, $"Loaded {localModels.Count} local models", syncStatusHandler);
                    syncCommand.NewerThan = syncStatus.NewestModifiedAt;
                    await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, true, _token);
                    
                    //then complete download of any older items still required
                    if (!syncRequirement.IsFulfilled(syncStatus.SyncedLocalEntities))
                    {
                        syncCommand.NewerThan = null;
                        syncCommand.OlderThan = syncStatus.OldestModifiedAt;
                        syncStatusHandler.State = SyncClientState.DownloadingOld;
                        await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, false, _token);
                    }
                    
                    syncStatusHandler.State = SyncClientState.Completed;
                }

                return new CompositeDisposable(loadNextBatchSubscription, loadNewFromServerSubscription, loadNewFromClientSubscription, reloadFromClientSubscription, cancelSubscription);
            });
        }

        public void LoadNextBatch()
        {
            LoadNextBatchEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private async Task LoadNextBatchAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                //TODO maybe cancel any existing  sync? Since we are using the same sync command there will be blood
                //todo actually maybe track a _lastPublishedLocally to use to load more locally and only load from server if no sync is in progress

                _numberOfModelsToPublish += syncCommand.BatchSize;
                if (syncStatusHandler.PublishedEntities + syncCommand.BatchSize <= syncStatusHandler.SyncedLocalEntities)
                {
                    //there are more local entities to load
                    TraceStatus(SyncClientState.LoadingLocal, $"LoadMore invoked. Loading next {syncCommand.BatchSize} of {_numberOfModelsToPublish} from device", syncStatusHandler);
                    syncCommand.OlderThan = OldestModelPublished;
                    syncCommand.NewerThan = null;
                    var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
                    PublishModelsIfRequired(localModels, observer, syncStatusHandler);
                }
                else
                {
                    // we need more from the server
                    TraceStatus(SyncClientState.DownloadingOld, $"LoadMore invoked. Loading next {syncCommand.BatchSize} of {_numberOfModelsToPublish} from server", syncStatusHandler);
                    syncCommand.OlderThan = OldestModelPublished;
                    syncCommand.NewerThan = null;
                    var serverModels = await _syncCommandHandler.HandleAsync(syncCommand);
                    PublishModelsIfRequired(serverModels.Value.EntityBatch, observer, syncStatusHandler);
                }

                TraceStatus(SyncClientState.Completed, $"LoadMore completed.", syncStatusHandler);
            } 
        }
        
        public void ReloadFromClient()
        {
            ReloadFromClientEvent?.Invoke(this, EventArgs.Empty);
        }

        private async Task ReloadFromClientAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, CancellationToken token)
        {
            TraceStatus(SyncClientState.LoadingLocal, $"Reload from client invoked. Loading all models from local store", syncStatusHandler);
            syncCommand.OlderThan = null;
            syncCommand.NewerThan = null;
            var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
            _publishedModels.Clear();
            PublishModelsIfRequired(localModels, observer, syncStatusHandler);
            TraceStatus(SyncClientState.Completed, $"Reload from client completed. {localModels.Count} loaded", syncStatusHandler);
        }

        public void LoadNewFromClient()
        {
            LoadNewFromClientEvent?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadNewFromClientAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, CancellationToken token)
        {
            TraceStatus(SyncClientState.LoadingLocal, $"Connect new from client invoked. Loading any updated models from local store", syncStatusHandler);
            ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);
            syncCommand.OlderThan = null;
            syncCommand.NewerThan = syncStatus.NewestModifiedAt;
            var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
            PublishModelsIfRequired(localModels, observer, syncStatusHandler);
            TraceStatus(SyncClientState.Completed, $"Connect new from client completed. {localModels.Count} loaded", syncStatusHandler);
        }

        public void Cancel()
        {
            CancelEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void Cancel(ISyncStatusHandler syncStatusHandler, CancellationToken token)
        {

            if (!token.IsCancellationRequested)
            {
                TraceStatus(SyncClientState.Cancelled, "Cancellation requested", syncStatusHandler);
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                _token = _cancellationTokenSource.Token;
            }
        }
        
        public void LoadNewFromServer()
        {
            LoadNewFromServerEvent?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadNewFromServerAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, CancellationToken token)
        {
            if (syncStatusHandler.IsExecuting())
            {
                _analyticsService.Trace(this, "ReloadFromServer invoked, but already busy: " + syncStatusHandler.State);
                return;
            }

            ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);
            TraceStatus(SyncClientState.DownloadingNew, $"ReloadFromServer invoked. Loading up to {syncCommand.BatchSize} new from server", syncStatusHandler);
            syncCommand.OlderThan = null;
            syncCommand.NewerThan = syncStatus.NewestModifiedAt;
            await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, true, token);
            TraceStatus(SyncClientState.Completed, $"ReloadFromServer completed", syncStatusHandler);
        }
       
        private async Task DownloadModelsAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, bool isLoadingNewerEntities, CancellationToken token)
        {
            while (true)
            {
                if (token == null || token.IsCancellationRequested)
                {
                    break;
                }
                var serverDownloadResult = await _syncCommandHandler.HandleAsync(syncCommand);
                if (serverDownloadResult.IsFailure)
                {
                    var errorMessage = $"Failed to load {typeof(TModel).Name} entities from server: " + serverDownloadResult.Error.Description;
                    
                    _analyticsService.TraceError(this, errorMessage);
                    syncStatusHandler.State = SyncClientState.Error;
                    syncStatusHandler.StatusMessage = errorMessage;

                    observer.OnError(new ErrorException(serverDownloadResult.Error));

                }

                PublishModelsIfRequired(serverDownloadResult.Value.EntityBatch, observer, syncStatusHandler);

                var syncResult = serverDownloadResult.Value;
                var updatedClientStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);

                syncStatusHandler.TotalEntitiesToDownload = syncResult.EntitiesToDownloadCount;
                syncStatusHandler.AllServerEntities = syncResult.TotalActiveEntityCount;
                syncStatusHandler.NewlyDownloadedEntities = syncStatusHandler.NewlyDownloadedEntities == null ? syncResult.EntityBatch.Count : syncStatusHandler.NewlyDownloadedEntities + syncResult.EntityBatch.Count;
                syncStatusHandler.AllLocalEntities = updatedClientStatus.AllLocalEntities;
                syncStatusHandler.SyncedLocalEntities = updatedClientStatus.SyncedLocalEntities;

                
                long stillToDownload = 0;

                //when loading newer items than what we have on device we always try and sync them all
                if (isLoadingNewerEntities || syncRequirement.SyncAll)
                {
                    stillToDownload = serverDownloadResult.Value.EntitiesToDownloadCount - serverDownloadResult.Value.EntityBatch.Count;
                }
                else if (syncRequirement.SyncMinimumQuantity != null)
                {
                    stillToDownload = syncRequirement.SyncMinimumQuantity.Value - updatedClientStatus.SyncedLocalEntities;
                }

                stillToDownload = Math.Max(0, stillToDownload);

                var newerOrOlder = isLoadingNewerEntities ? "newer" : "older";
                var syncState = isLoadingNewerEntities ? SyncClientState.DownloadingNew : SyncClientState.DownloadingOld;
                TraceStatus(syncState, $"{syncResult.EntityBatch.Count} {newerOrOlder} {typeof(TModel).Name} entities downloaded ({syncStatusHandler.NewlyDownloadedEntities} in total). {stillToDownload} of {syncResult.TotalActiveEntityCount} still to download", syncStatusHandler);

                if (stillToDownload > 0)
                {
                    if (isLoadingNewerEntities)
                    {
                        syncCommand.NewerThan = serverDownloadResult.Value.EntityBatch.First().ModifiedAtTicks;
                        syncCommand.OlderThan = null;
                    }
                    else
                    {
                        syncCommand.NewerThan = null;
                        syncCommand.OlderThan = serverDownloadResult.Value.EntityBatch.Last().ModifiedAtTicks;
                    }
                    continue;
                }

                return;
            }
        }

        private void PublishModelsIfRequired(IEnumerable<TModel> models, IObserver<TModel> observer,  ISyncStatusHandler syncStatusHandler)
        {
            foreach (var model in models)
            {
                if (model.IsNewerThan(NewestModelPublished))
                {
                    //always publish model if newer than what has already been published
                    _publishedModels[model.Id] = model.ModifiedAtTicks;
                    syncStatusHandler.PublishedEntities = _publishedModels.Count;
                    _numberOfModelsToPublish = Math.Max(_publishedModels.Count, _numberOfModelsToPublish);
                    observer.OnNext(model);
                }
                else if (syncStatusHandler.PublishedEntities < _numberOfModelsToPublish)
                {
                    //only publish models older than what we have already if the requirement has increased by LoadMore
                    _publishedModels[model.Id] = model.ModifiedAtTicks;
                    syncStatusHandler.PublishedEntities = _publishedModels.Count;
                    observer.OnNext(model);
                }
            }
        }
        
        private void TraceStatus(SyncClientState state, string message, ISyncStatusHandler statusHandler)
        {
            statusHandler.StatusMessage = message;
            statusHandler.State = state;
            _analyticsService.TraceVerbose(this, $"{typeof(TModel).Name} {state}: {message}");
        }
         

    }
}