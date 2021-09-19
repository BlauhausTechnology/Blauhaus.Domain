using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync.SyncClient
{
    public interface IDtoSyncClient : IAsyncPublisher<DtoSyncStatus>
    {
        Task<KeyValuePair<string, long>> LoadLastModifiedTicksAsync();
        Task<Response> SyncDtoAsync(int batchSize, Dictionary<string, long>? dtosLastModifiedTicks);
    }
}