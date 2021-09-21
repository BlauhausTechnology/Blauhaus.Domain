using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientActors.Actors;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync
{
    public class DtoSyncHandler<TDto, TId> : BaseActor, IDtoSyncHandler
        where TDto : class, IClientEntity<TId>
        where TId : IEquatable<TId>
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISyncDtoCache<TDto, TId> _syncDtoCache;
        private readonly ICommandHandler<IDtoBatch<TDto>, DtoSyncCommand> _syncCommandHandler;

        protected string DtoName = typeof(TDto).Name;

        public DtoSyncHandler(
            IAnalyticsService analyticsService,
            ISyncDtoCache<TDto, TId> syncDtoCache,
            ICommandHandler<IDtoBatch<TDto>, DtoSyncCommand> syncCommandHandler)
        {
            _analyticsService = analyticsService;
            _syncDtoCache = syncDtoCache;
            _syncCommandHandler = syncCommandHandler;
        }
         
        public Task<Response> SyncDtoAsync(IKeyValueProvider? settingsProvider)
        {
            return InvokeAsync(async () =>
            {
                var lastModifiedTicks = await _syncDtoCache.LoadLastModifiedTicksAsync(settingsProvider);

                var syncCommand = DtoSyncCommand.Create<TDto>(lastModifiedTicks);

                var syncResult = await _syncCommandHandler.HandleAsync(syncCommand);
                if (syncResult.IsFailure)
                {
                    return Response.Failure(syncResult.Error);
                }
                var dtoSyncStatus = DtoSyncStatus.Create(DtoName, syncResult.Value);

                await UpdateSubscribersAsync(dtoSyncStatus);

                while (dtoSyncStatus.RemainingDtoCount > 0)
                {
                    syncResult = await _syncCommandHandler.HandleAsync(DtoSyncCommand.Create<TDto>(syncResult.Value.BatchLastModifiedTicks));
                    if (syncResult.IsFailure)
                    {
                        return Response.Failure(syncResult.Error);
                    }

                    dtoSyncStatus = dtoSyncStatus.Update(syncResult.Value);
                    await UpdateSubscribersAsync(dtoSyncStatus);
                }

                return Response.Success();
            });

        }
        
        public Task<IDisposable> SubscribeAsync(Func<DtoSyncStatus, Task> handler, Func<DtoSyncStatus, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

    }
}