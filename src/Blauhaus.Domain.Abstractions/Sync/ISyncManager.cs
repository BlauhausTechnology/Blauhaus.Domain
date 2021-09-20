using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.Sync
{

    public interface ISyncManager : IAsyncPublisher<IOverallSyncStatus>
    {
        Task<Dictionary<string, long>> GetLastModifiedTicksAsync(IKeyValueProvider? settingsProvider);
        Task<Response> SyncAllAsync(Dictionary<string, long>? dtosLastModifiedTicks, IKeyValueProvider? settingsProvider);
        Task<Response> SyncAllAsync(IKeyValueProvider? settingsProvider = null);
    }
}