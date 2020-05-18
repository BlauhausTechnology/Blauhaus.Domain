using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Time.Service;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Common.Extensions;

namespace Blauhaus.Domain.Client.Sync
{
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
            ITimeService timeService,
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
            TraceStatus(SyncClientState.Starting, $"{typeof(TModel).Name} SyncClient connected. Required: {syncRequirement} (batch size {syncCommand.BatchSize})", syncStatusHandler, new Dictionary<string, object>
            {
                {"SyncCommand", syncCommand},
                {"SyncRequirement", syncRequirement}
            });

            _numberOfModelsToPublish = syncCommand.BatchSize;
            syncStatusHandler.PublishedEntities = 0;

            return Observable.Create<TModel>(async observer =>
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _token = _cancellationTokenSource.Token;
                
                ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);

                TraceStatus(SyncClientState.Starting, $"Initializing sync for {typeof(TModel).Name}. Local status {syncStatus}", syncStatusHandler, syncStatus.ToObjectDictionary(nameof(ClientSyncStatus)));
                
                syncStatusHandler.IsConnected = _connectivityService.IsConnectedToInternet;
                syncStatusHandler.AllLocalEntities = syncStatus.AllLocalEntities;
                syncStatusHandler.SyncedLocalEntities = syncStatus.SyncedLocalEntities;

                async void HandleLoadNextBatch(object s, EventArgs e)
                    => await LoadNextBatchAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);
                
                async void HandleLoadNewFromClient(object s, EventArgs e)
                    => await LoadNewFromClientAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);

                async void HandleLoadNewFromServer(object s, EventArgs e)
                    => await LoadNewFromServerAsync(syncCommand, syncRequirement, syncStatusHandler, observer, _token);

                void HandleCancel(object s, EventArgs e)
                    => Cancel(syncStatusHandler, _token);

                var loadNextBatchSubscription = Observable.FromEventPattern(x => LoadNextBatchEvent += HandleLoadNextBatch, x => LoadNextBatchEvent -= HandleLoadNextBatch).Subscribe();
                var loadNewFromServerSubscription = Observable.FromEventPattern(x => LoadNewFromServerEvent += HandleLoadNewFromServer, x => LoadNewFromServerEvent -= HandleLoadNewFromServer).Subscribe();
                var loadNewFromClientSubscription = Observable.FromEventPattern(x => LoadNewFromClientEvent += HandleLoadNewFromClient, x => LoadNewFromClientEvent -= HandleLoadNewFromClient).Subscribe();
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

                return new CompositeDisposable(loadNextBatchSubscription, loadNewFromServerSubscription, loadNewFromClientSubscription, cancelSubscription);
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
                    var serverModels = await _syncCommandHandler.HandleAsync(syncCommand, token);
                    PublishModelsIfRequired(serverModels.Value.EntityBatch, observer, syncStatusHandler);
                }

                TraceStatus(SyncClientState.Completed, $"LoadMore completed.", syncStatusHandler);
            } 
        }

        public void LoadNewFromClient()
        {
            LoadNewFromClientEvent?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadNewFromClientAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, CancellationToken token)
        {
            TraceStatus(SyncClientState.LoadingLocal, $"Load new from client invoked. Loading any updated models from local store", syncStatusHandler);
            ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);
            syncCommand.OlderThan = null;
            syncCommand.NewerThan = syncStatus.NewestModifiedAt;
            var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
            PublishModelsIfRequired(localModels, observer, syncStatusHandler);
            TraceStatus(SyncClientState.Completed, $"Load new from client completed. {localModels.Count} loaded", syncStatusHandler);
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
            ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);
            TraceStatus(SyncClientState.DownloadingNew, $"Refresh invoked. Loading up to {syncCommand.BatchSize} new from server", syncStatusHandler);
            syncCommand.OlderThan = null;
            syncCommand.NewerThan = syncStatus.NewestModifiedAt;
            await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, true, token);
            TraceStatus(SyncClientState.Completed, $"Refresh completed", syncStatusHandler);
        }
       
        private async Task DownloadModelsAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, bool isLoadingNewerEntities, CancellationToken token)
        {
            while (true)
            {
                if (token == null || token.IsCancellationRequested)
                {
                    break;
                }
                var serverDownloadResult = await _syncCommandHandler.HandleAsync(syncCommand, token);
                if (serverDownloadResult.IsFailure)
                {
                    observer.OnError(HandleError($"Failed to load {typeof(TModel).Name} entities from server: " + serverDownloadResult.Error, syncStatusHandler));
                }

                PublishModelsIfRequired(serverDownloadResult.Value.EntityBatch, observer, syncStatusHandler);

                var syncResult = serverDownloadResult.Value;
                var updatedClientStatus = await _syncClientRepository.GetSyncStatusAsync(syncCommand);

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
        
        private void TraceStatus(SyncClientState state, string message, ISyncStatusHandler statusHandler, Dictionary<string, object>? properties = null)
        {
            statusHandler.StatusMessage = message;
            statusHandler.State = state;
            _analyticsService.TraceVerbose(this, $"{state}: {message}", properties ?? statusHandler.ToObjectDictionary("SyncStatus"));
        }

        private Exception HandleError(string error, ISyncStatusHandler syncStatusHandler)
        {
            _analyticsService.TraceError(this, error);
            syncStatusHandler.State = SyncClientState.Error;
            syncStatusHandler.StatusMessage = error;
            return new Exception(error);
        }

    }
}