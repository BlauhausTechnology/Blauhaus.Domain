using Blauhaus.Domain.Client.Sync.Client;

namespace Blauhaus.Domain.Client.Sync.Service
{
    public interface ISyncStatusHandlerFactory
    {
        public ISyncStatusHandler Get();
    }
}