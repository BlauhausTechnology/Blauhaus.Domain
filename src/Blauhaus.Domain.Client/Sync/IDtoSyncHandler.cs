using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync
{
    public interface IDtoSyncHandler : IAsyncPublisher<DtoSyncStatus>
    {
        Task<Response> SyncDtoAsync(IKeyValueProvider? settingsProvider);
    }
}