namespace Blauhaus.Domain.Client.Sync
{
    public class ClientSyncStatus
    {
        public long? LastModifiedAt { get; set; }
        public long FirstModifiedAt { get; set; }
        public long TotalCount { get; set; }
    }
}