using System;
using System.Collections.Generic;
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

        public static SyncCommand WithFilter(this SyncCommand syncCommand, string name, object value)
        {
            syncCommand.Filters ??= new Dictionary<string, object>();
            syncCommand.Filters[name] = value;
            return syncCommand;
        }
        
        public static bool TryGetFilterValue<TValue>(this SyncCommand syncCommand, string name, out TValue? value) where TValue : class
        {
            value = null;

            if (syncCommand.Filters != null)
            {
                foreach (var syncCommandFilter in syncCommand.Filters)
                {
                    if (syncCommandFilter.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = syncCommandFilter.Value as TValue;
                        if (value != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}