using System;

namespace Blauhaus.Domain.Common.CommandHandlers.Sync
{
    public class SyncCommand
    {

        public long? ModifiedAfterTicks { get; set; }
        public long ModifiedBeforeTicks { get; set; }
        public int BatchSize { get; set; } = 100;

    }
}