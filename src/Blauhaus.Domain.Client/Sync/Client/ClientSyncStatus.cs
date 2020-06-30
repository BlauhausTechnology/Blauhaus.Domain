namespace Blauhaus.Domain.Client.Sync.Client
{
    public class ClientSyncStatus
    {
        public long? NewestModifiedAt { get; set; }
        public long OldestModifiedAt { get; set; }
        public long AllLocalEntities { get; set; }
        public long SyncedLocalEntities { get; set; }

        public override string ToString()
        {
            return $"Synced: {SyncedLocalEntities}. (total: {AllLocalEntities})";
        }
    }
}