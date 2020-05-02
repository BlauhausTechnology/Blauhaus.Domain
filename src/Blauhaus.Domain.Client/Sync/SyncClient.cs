using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
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
        private readonly ISyncClientRepository<TModel, TDto> _syncClientRepository;
        private readonly ICommandHandler<SyncResult<TModel>, TSyncCommand> _syncCommandHandler;

        public SyncClient(
            IAnalyticsService analyticsService, 
            ISyncClientRepository<TModel, TDto> syncClientRepository,
            ICommandHandler<SyncResult<TModel>, TSyncCommand> syncCommandHandler)
        {
            _analyticsService = analyticsService;
            _syncClientRepository = syncClientRepository;
            _syncCommandHandler = syncCommandHandler;
        }


        public IObservable<SyncUpdate<TModel>> Connect(TSyncCommand syncCommand)
        {
            _analyticsService.TraceVerbose(this, $"{typeof(TModel).Name} SyncClient connected", syncCommand.ToObjectDictionary());

            return Observable.Create<SyncUpdate<TModel>>(async observer =>
            {
                var disposable = new CancellationDisposable();
                
                ClientSyncStatus syncStatus = await _syncClientRepository.GetSyncStatusAsync();

                var firstBatchOfLocalModels= await _syncClientRepository.LoadOlderItemsAsync(null, syncCommand.BatchSize);
                foreach (var localModel in firstBatchOfLocalModels)
                {
                    observer.OnNext(new SyncUpdate<TModel>(localModel));
                }

                _analyticsService.TraceVerbose(this, "Initial batch of local models loaded", new Dictionary<string, object>
                {
                    {nameof(ClientSyncStatus), syncStatus},
                    {"Models published", firstBatchOfLocalModels.Count}
                });

                syncCommand.ModifiedBeforeTicks = syncStatus.FirstModifiedAt;
                syncCommand.ModifiedAfterTicks = syncStatus.LastModifiedAt;

                var updatesFromServer = await _syncCommandHandler.HandleAsync(syncCommand, disposable.Token);

                _analyticsService.TraceVerbose(this, "Sync result received from server", new Dictionary<string, object>
                {
                    { nameof(SyncResult<TModel>.ModifiedEntityCount), updatesFromServer.Value.ModifiedEntityCount },
                    { nameof(SyncResult<TModel>.TotalEntityCount), updatesFromServer.Value.TotalEntityCount },
                    { nameof(SyncResult<TModel>.Entities), updatesFromServer.Value.Entities.Count },
                });

                foreach (var serverModel in updatesFromServer.Value.Entities)
                {
                    observer.OnNext(new SyncUpdate<TModel>(serverModel));
                }
                
                return disposable;
            });
        }
    }
}