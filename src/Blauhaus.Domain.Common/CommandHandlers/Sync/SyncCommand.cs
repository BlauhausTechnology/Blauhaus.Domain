using System;
using System.Collections.Generic;

namespace Blauhaus.Domain.Common.CommandHandlers.Sync
{
    public class SyncCommand
    {

        /// <summary>
        /// Used only when asking for entities that have been modified after the last sync. Returns older entities first
        /// </summary>
        public long? NewerThan { get; set; }

        /// <summary>
        /// Used when syncing additional entities after an initial set has been downloaded. Returns newer entities first. 
        /// </summary>
        public long? OlderThan { get; set; }
        
        /// <summary>
        /// Specifies the number of entities to be downloaded from the server and published to the client in each batch
        /// </summary>
        public int BatchSize { get; set; } = 100;
        

    }
}