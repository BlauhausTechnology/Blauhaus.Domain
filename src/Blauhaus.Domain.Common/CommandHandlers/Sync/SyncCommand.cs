using System;

namespace Blauhaus.Domain.Common.CommandHandlers.Sync
{
    public class SyncCommand
    {

        public long? NewerThan { get; set; }
        public long? OlderThan { get; set; }
        public int BatchSize { get; set; } = 100;

    }
}