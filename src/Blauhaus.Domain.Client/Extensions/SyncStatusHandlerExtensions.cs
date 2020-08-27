using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.Client;

namespace Blauhaus.Domain.Client.Extensions
{
    public static class SyncStatusHandlerExtensions
    {
        public static bool IsExecuting(this ISyncStatusHandler syncStatusHandler)
        {
            return syncStatusHandler.State.IsExecuting();
        }
    }
}