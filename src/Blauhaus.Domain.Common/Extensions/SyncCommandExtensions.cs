using Blauhaus.Domain.Common.CommandHandlers.Sync;

namespace Blauhaus.Domain.Common.Extensions
{
    public static class SyncCommandExtensions
    {
        public static bool IsFirstSyncForDevice(this SyncCommand syncCommand)
        {
            return (syncCommand.ModifiedAfterTicks == null || syncCommand.ModifiedAfterTicks.Value == 0)
                   && syncCommand.ModifiedBeforeTicks == 0;
        }

        public static bool IsFirstRequestInSyncSequence(this SyncCommand syncCommand)
        {
            return syncCommand.ModifiedAfterTicks != null && syncCommand.ModifiedBeforeTicks != 0;
        }
    }
}