using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Client.Sync.Old.Service
{
    public class SyncUpdateState
    {

        private readonly Dictionary<int, bool> _entityTypesSynced = new Dictionary<int, bool>();

        private readonly Dictionary<int, long> _entitiesToSync = new Dictionary<int, long>();
        private readonly Dictionary<int, long> _entitiesSynced = new Dictionary<int, long>();

        public SyncUpdateState(int syncClientsCount)
        {
            EntityTypesToSync = syncClientsCount;
        }


        public int EntityTypesToSync { get; }
        public int EntityTypesSynced => _entityTypesSynced.Values.Count(x => true);

        public long EntitiesToSync => _entitiesToSync.Values.Sum();
        public long EntitiesSynced => _entitiesSynced.Values.Sum();

        public SyncUpdate Publish() => new SyncUpdate(
            EntityTypesToSync, EntityTypesSynced,
            EntitiesToSync, EntitiesSynced);

        public SyncUpdate Update(int id, ISyncStatusHandler syncStatus)
        {
            if (syncStatus.State == SyncClientState.Completed)
            {
                _entityTypesSynced[id] = true;
            }

            if (syncStatus.TotalEntitiesToDownload != null && syncStatus.TotalEntitiesToDownload > 0)
            {
                _entitiesToSync[id] = syncStatus.TotalEntitiesToDownload.Value;
            }

            if (syncStatus.NewlyDownloadedEntities != null && syncStatus.NewlyDownloadedEntities.Value > 0)
            {
                _entitiesSynced[id] = syncStatus.NewlyDownloadedEntities.Value;
            }

            return Publish();
        }
    }
}