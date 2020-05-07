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
            _analyticsService.TraceVerbose(this, $"{typeof(TModel).Name} SyncClient connected", syncCommand.ToObjectDictionary());

            return Observable.Create<TModel>(async observer =>
            {
                var disposable = new CancellationDisposable();
                
                ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync();

                _analyticsService.TraceVerbose(this, $"Initializing sync for {typeof(TModel).Name}", syncStatus.ToObjectDictionary(nameof(ClientSyncStatus)));
                syncStatusHandler.IsConnected = _connectivityService.IsConnectedToInternet;
                syncStatusHandler.AllLocalEntities = syncStatus.AllLocalEntities;
                syncStatusHandler.SyncedLocalEntities = syncStatus.SyncedLocalEntities;

                if (syncStatus.SyncedLocalEntities == 0)
                {
                    //first time sync
                    syncCommand.NewerThan = null;
                    syncCommand.OlderThan = null;

                    await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, false, disposable.Token);
                }

                else
                {
                    //first update any new or modified entities
                    syncCommand.NewerThan = syncStatus.NewestModifiedAt;
                    await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, true, disposable.Token);
                    
                    //then complete download of any older items still required
                    if (!syncRequirement.IsFulfilled(syncStatus.SyncedLocalEntities))
                    {
                        syncCommand.NewerThan = null;
                        syncCommand.OlderThan = syncStatus.OldestModifiedAt;
                        await DownloadModelsAsync(syncCommand, syncRequirement, syncStatusHandler, observer, false, disposable.Token);
                    }
                }

                return disposable;
            });
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

                foreach (var newModel in serverDownloadResult.Value.EntityBatch)
                {
                    observer.OnNext(newModel);
                }

                var syncResult = serverDownloadResult.Value;
                var updatedClientStatus = await _syncClientRepository.GetSyncStatusAsync();

                syncStatusHandler.AllServerEntities = syncResult.TotalActiveEntityCount;
                syncStatusHandler.LoadedEntities = syncStatusHandler.LoadedEntities == null ? syncResult.EntityBatch.Count : syncStatusHandler.LoadedEntities + syncResult.EntityBatch.Count;
                syncStatusHandler.NewlyDownloadedEntities = syncStatusHandler.NewlyDownloadedEntities == null ? syncResult.EntityBatch.Count : syncStatusHandler.NewlyDownloadedEntities + syncResult.EntityBatch.Count;
                syncStatusHandler.AllLocalEntities = updatedClientStatus.AllLocalEntities;
                syncStatusHandler.SyncedLocalEntities = updatedClientStatus.SyncedLocalEntities;

                _analyticsService.TraceVerbose(this, $"{syncResult.EntityBatch.Count} {typeof(TModel).Name} entities downloaded", syncStatusHandler.ToObjectDictionary("SyncStatus"));

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

        private Exception HandleError(string error, ISyncStatusHandler syncStatusHandler)
        {
            _analyticsService.TraceError(this, error);
            syncStatusHandler.StatusMessage = error;
            return new Exception(error);
        }
    }
}