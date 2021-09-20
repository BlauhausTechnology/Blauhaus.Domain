﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientActors.Actors;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync.SyncClient
{
    public class DtoSyncClient<TDto, TId> : BaseActor, IDtoSyncClient
        where TDto : class, IClientEntity<TId>
        where TId : IEquatable<TId>
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISyncDtoCache<TDto, TId> _syncDtoCache;
        private readonly ICommandHandler<IDtoBatch<TDto>, DtoSyncCommand> _syncCommandHandler;

        protected string DtoName = typeof(TDto).Name;

        public DtoSyncClient(
            IAnalyticsService analyticsService,
            ISyncDtoCache<TDto, TId> syncDtoCache,
            ICommandHandler<IDtoBatch<TDto>, DtoSyncCommand> syncCommandHandler)
        {
            _analyticsService = analyticsService;
            _syncDtoCache = syncDtoCache;
            _syncCommandHandler = syncCommandHandler;
        }
        
        public Task<KeyValuePair<string, long>> LoadLastModifiedTicksAsync(IKeyValueProvider? settingsProvider)
        {
            return InvokeAsync(async () =>
            {
                var lastModified = await _syncDtoCache.LoadLastModifiedTicksAsync(settingsProvider);
                return new KeyValuePair<string, long>(DtoName, lastModified);
            });
        }

        public Task<Response> SyncDtoAsync(Dictionary<string, long>? dtosLastModifiedTicks, IKeyValueProvider? settingsProvider)
        {
            return InvokeAsync(async () =>
            {
                if (dtosLastModifiedTicks == null || !dtosLastModifiedTicks.TryGetValue(DtoName, out var lastModifiedTicks))
                {
                    lastModifiedTicks = await _syncDtoCache.LoadLastModifiedTicksAsync(settingsProvider);
                }

                var syncCommand = new DtoSyncCommand(lastModifiedTicks);

                var syncResult = await _syncCommandHandler.HandleAsync(syncCommand);
                if (syncResult.IsFailure)
                {
                    return Response.Failure(syncResult.Error);
                }
                var dtoSyncStatus = DtoSyncStatus.Create(DtoName, syncResult.Value);

                await UpdateSubscribersAsync(dtoSyncStatus);

                while (dtoSyncStatus.RemainingDtoCount > 0)
                {
                    syncResult = await _syncCommandHandler.HandleAsync(new DtoSyncCommand(syncResult.Value.BatchLastModifiedTicks));
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