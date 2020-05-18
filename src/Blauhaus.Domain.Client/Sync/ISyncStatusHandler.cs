﻿namespace Blauhaus.Domain.Client.Sync
{
    public interface ISyncStatusHandler
    {
        public long? AllLocalEntities { get; set; }

        public long? SyncedLocalEntities{ get; set; }

        public long? AllServerEntities{ get; set; }
        
        public long? NewlyDownloadedEntities{ get; set; }

        public long? PublishedEntities{ get; set; }
        
        public bool IsConnected{ get; set; }

        public string StatusMessage { get; set; }
        public SyncClientState State { get; set; }

    }
}