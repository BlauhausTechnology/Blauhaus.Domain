namespace Blauhaus.Domain.Abstractions.Sync.Old
{
    public class SyncUpdate
    {
        public SyncUpdate(long entityTypesToSync, long entityTypesSynced, long entitiesToSync, long entitiesSynced)
        {
            EntityTypesToSync = entityTypesToSync;
            EntityTypesSynced = entityTypesSynced;
            EntitiesToSync = entitiesToSync;
            EntitiesSynced = entitiesSynced;
        }

        public long EntityTypesToSync { get; }
        public long EntityTypesSynced { get; }
        public float EntityTypeSyncProgress => EntityTypesSynced == 0 ? 0 : EntityTypesSynced / (float) EntityTypesToSync;

        public long EntitiesToSync { get; }
        public long EntitiesSynced { get; }
        public float EntitySyncProgress => EntitiesToSync == 0 ? 0 :  EntitiesSynced / (float) EntitiesToSync;

        public bool IsCompleted => EntityTypeSyncProgress == 1;


    }
}