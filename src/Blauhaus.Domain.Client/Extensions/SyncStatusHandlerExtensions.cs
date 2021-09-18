using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Abstractions.Sync.Old;

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