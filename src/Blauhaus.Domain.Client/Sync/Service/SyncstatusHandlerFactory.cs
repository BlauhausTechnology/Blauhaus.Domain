using Blauhaus.Domain.Client.Sync.Client;

namespace Blauhaus.Domain.Client.Sync.Service
{
    public class SyncstatusHandlerFactory : ISyncStatusHandlerFactory
    {
        public ISyncStatusHandler Get()
        {
            return new SyncStatusHandler();
        }
    }
}