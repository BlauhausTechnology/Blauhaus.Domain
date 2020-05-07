using Blauhaus.Domain.Common.CommandHandlers.Sync;

namespace Blauhaus.Domain.Common.Extensions
{
    public static class SyncCommandExtensions
    {
        public static bool IsFirstSyncForDevice(this SyncCommand syncCommand)
        {
            return syncCommand.NewerThan == null && syncCommand.OlderThan == null;
        }
         
        public static bool IsForNewerEntities(this SyncCommand syncCommand)
        {
            return syncCommand.NewerThan != null;
        }
        
        public static bool IsForOlderEntities(this SyncCommand syncCommand)
        {
            return syncCommand.OlderThan != null;
        }
    }
}