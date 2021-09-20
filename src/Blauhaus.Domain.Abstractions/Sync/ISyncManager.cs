using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.Sync
{

    public interface ISyncManager : IAsyncPublisher<IOverallSyncStatus>
    {
        Task<Response> SyncAllAsync(IKeyValueProvider? settingsProvider);
    }
}