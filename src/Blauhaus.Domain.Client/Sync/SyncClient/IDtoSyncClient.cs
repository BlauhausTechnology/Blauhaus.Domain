using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync.SyncClient
{
    public interface IDtoSyncClient : IAsyncPublisher<DtoSyncStatus>
    {
        Task<Response> SyncDtoAsync(IKeyValueProvider? settingsProvider);
    }
}