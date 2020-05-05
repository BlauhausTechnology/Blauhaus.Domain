namespace Blauhaus.Domain.Client.Sync
{
    public class ClientSyncStatus
    {
        public long? LastModifiedAt { get; set; }
        public long FirstModifiedAt { get; set; }
        public long LocalEntities { get; set; }
        public long LocalSyncedEntities { get; set; }
    }
}