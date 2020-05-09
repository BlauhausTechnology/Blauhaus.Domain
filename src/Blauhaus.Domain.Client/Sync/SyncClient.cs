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

        private event EventHandler LoadMoreEvent;

        private long _numberOfModelsToPublish;
        private long _oldestModelPublished;

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
            TraceStatus(syncStatusHandler,  $"{typeof(TModel).Name} SyncClient connected. Required: {syncRequirement} (batch size {syncCommand.BatchSize})", new Dictionary<string, object>
            {
                {"SyncCommand", syncCommand},
                {"SyncRequirement", syncRequirement}
            });

            _numberOfModelsToPublish = syncCommand.BatchSize;
            syncStatusHandler.PublishedEntities = 0;

            return Observable.Create<TModel>(async observer =>
            {
                var cancellation = new CancellationDisposable();
                
                ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync();

                TraceStatus(syncStatusHandler, $"Initializing sync for {typeof(TModel).Name}. Local status {syncStatus}", syncStatus.ToObjectDictionary(nameof(ClientSyncStatus)));
                
                syncStatusHandler.IsConnected = _connectivityService.IsConnectedToInternet;
                syncStatusHandler.AllLocalEntities = syncStatus.AllLocalEntities;
                syncStatusHandler.SyncedLocalEntities = syncStatus.SyncedLocalEntities;

                async void HandleLoadMore(object s, EventArgs e)
                {
                    //maybe cancel any existing background sync? Since we are using the same sync command there will be blood

                    _analyticsService.TraceVerbose(this, "LoadMore invoked", syncStatusHandler.ToObjectDictionary());
                    _numberOfModelsToPublish += syncCommand.BatchSize;
                    if (syncStatusHandler.PublishedEntities + syncCommand.BatchSize <= syncStatusHandler.SyncedLocalEntities)
                    {
                        //there are more local entities to load
                        syncCommand.OlderThan = _oldestModelPublished;
                        syncCommand.NewerThan = null;
                        var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
                        PublishModelsIfRequired(localModels, observer, syncStatusHandler);
                    }
                    else
                    {
                        // we need more from the server
                        syncCommand.OlderThan = _oldestModelPublished;
                        syncCommand.NewerThan = null;
                        var serverModels = await _syncCommandHandler.HandleAsync(syncCommand, CancellationToken.None);
                        PublishModelsIfRequired(serverModels.Value.EntityBatch, observer, syncStatusHandler);
                    }
                }

                var loadMoreSubscription = Observable.FromEventPattern(x => LoadMoreEvent += HandleLoadMore, x => LoadMoreEvent -= HandleLoadMore).Subscribe();

                if (syncStatus.SyncedLocalEntities == 0)
                {
                    TraceStatus(syncStatusHandler, "No local data, checking server...");
                    syncCommand.NewerThan = null;
                    syncCommand.OlderThan = null;
                    syncStatusHandler.State = SyncClientState.DownloadingOld;

                    await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, false, cancellation.Token);

                    syncStatusHandler.State = SyncClientState.Completed;
                }

                else
                {
                    //first load and publish local models
                    syncStatusHandler.State = SyncClientState.LoadingLocal;
                    TraceStatus(syncStatusHandler, "Loading data from local store");
                    var localModels = await _syncClientRepository.LoadModelsAsync(syncCommand);
                    PublishModelsIfRequired(localModels, observer, syncStatusHandler);

                    TraceStatus(syncStatusHandler, $"Loaded {localModels.Count} local models");

                    //first update any new or modified entities
                    syncStatusHandler.State = SyncClientState.DownloadingNew;
                    syncCommand.NewerThan = syncStatus.NewestModifiedAt;
                    await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, true, cancellation.Token);
                    
                    //then complete download of any older items still required
                    if (!syncRequirement.IsFulfilled(syncStatus.SyncedLocalEntities))
                    {
                        syncCommand.NewerThan = null;
                        syncCommand.OlderThan = syncStatus.OldestModifiedAt;
                        syncStatusHandler.State = SyncClientState.DownloadingOld;
                        await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, false, cancellation.Token);
                    }
                    
                    syncStatusHandler.State = SyncClientState.Completed;
                }

                return new CompositeDisposable(cancellation, loadMoreSubscription);
            });
        }

        public void LoadMore()
        {
            LoadMoreEvent?.Invoke(this, EventArgs.Empty);
        }

        private void TraceStatus(ISyncStatusHandler statusHandler, string message, Dictionary<string, object>? properties = null)
        {
            statusHandler.StatusMessage = message;
            _analyticsService.TraceVerbose(this, message, properties ?? statusHandler.ToObjectDictionary("SyncStatus"));
        }


        private async Task DownloadModelsAsync(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler, IObserver<TModel> observer, bool isLoadingNewerEntities, CancellationToken token)
        {
            while (true)
            {
                var serverDownloadResult = await _syncCommandHandler.HandleAsync(syncCommand, token);
                if (serverDownloadResult.IsFailure)
                {
                    observer.OnError(HandleError($"Failed to load {typeof(TModel).Name} entities from server: " + serverDownloadResult.Error, syncStatusHandler));
                }

                PublishModelsIfRequired(serverDownloadResult.Value.EntityBatch, observer, syncStatusHandler);

                var syncResult = serverDownloadResult.Value;
                var updatedClientStatus = await _syncClientRepository.GetSyncStatusAsync();

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
                TraceStatus(syncStatusHandler, $"{syncResult.EntityBatch.Count} {newerOrOlder} {typeof(TModel).Name} entities downloaded ({syncStatusHandler.NewlyDownloadedEntities} in total). {stillToDownload} of {syncResult.TotalActiveEntityCount} still to download");

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

        private void PublishModelsIfRequired(IEnumerable<TModel> models,IObserver<TModel> observer,  ISyncStatusHandler syncStatusHandler)
        {
            foreach (var localModel in models)
            {
                if (syncStatusHandler.PublishedEntities < _numberOfModelsToPublish)
                {
                    syncStatusHandler.PublishedEntities++;
                    if (_oldestModelPublished == 0 || localModel.ModifiedAtTicks < _oldestModelPublished)
                    {
                        _oldestModelPublished = localModel.ModifiedAtTicks;
                    }
                    observer.OnNext(localModel);
                }
            }
        }

        private Exception HandleError(string error, ISyncStatusHandler syncStatusHandler)
        {
            _analyticsService.TraceError(this, error);
            syncStatusHandler.StatusMessage = error;
            return new Exception(error);
        }

    }
}