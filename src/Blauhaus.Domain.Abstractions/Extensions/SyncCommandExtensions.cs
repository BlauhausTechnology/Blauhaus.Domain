using System;
using SyncCommand = Blauhaus.Domain.Abstractions.Sync.SyncCommand;

namespace Blauhaus.Domain.Abstractions.Extensions
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
        public static bool IsForSingleEntity(this SyncCommand syncCommand)
        {
            return syncCommand.Id != null && syncCommand.Id != default(Guid);
        }
      
    }
}