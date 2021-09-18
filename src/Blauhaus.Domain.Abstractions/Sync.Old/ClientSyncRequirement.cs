namespace Blauhaus.Domain.Abstractions.Sync.Old
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

        public static ClientSyncRequirement Minimum(long minimumQuantity) => new ClientSyncRequirement(false, minimumQuantity);
        public static readonly ClientSyncRequirement All = new ClientSyncRequirement(true, null);
        public static readonly ClientSyncRequirement Batch = new ClientSyncRequirement(false, null);

        public override string ToString()
        {
            if (SyncAll)
            {
                return "All";
            }

            if (SyncMinimumQuantity == null)
            {
                return "Batch";
            }

            return "Minimum " + SyncMinimumQuantity.Value;
        }

        public bool IsFulfilled(long? syncedLocalEntities)
        {

            if (!SyncAll)
            {
                if (SyncMinimumQuantity == null)
                {
                    if (syncedLocalEntities != null && syncedLocalEntities.Value > 0)
                    {
                        //we only require one batch to be synced
                        return true;
                    }
                }
                else
                {
                    if (syncedLocalEntities != null && syncedLocalEntities.Value >= SyncMinimumQuantity.Value)
                    {
                        //we have fulfilled the minimum requirement
                        return true;
                    }
                }
            }

            return false;
        }
    }
}