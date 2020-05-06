namespace Blauhaus.Domain.Client.Sync
{
    public class ClientSyncRequirement
    {
        private ClientSyncRequirement(bool syncAll, long? syncMinimumQuantity)
        {
            SyncAll = syncAll;
            SyncMinimumQuantity = syncMinimumQuantity;
        }

        public bool SyncAll { get; }
        public long? SyncMinimumQuantity { get; }

        public static ClientSyncRequirement AtLeast(long minimumQuantity) => new ClientSyncRequirement(false, minimumQuantity);
        public static ClientSyncRequirement All = new ClientSyncRequirement(true, null);
        public static ClientSyncRequirement Batch = new ClientSyncRequirement(false, null);
    }
}