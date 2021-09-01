using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Entities
{
    public abstract class SyncClientEntity<TId> : ClientEntity<TId>, ISyncClientEntity<TId>
    {
        [Indexed]
        public SyncState SyncState { get; set; }

        public string SerializedDto { get; set; } = string.Empty;
    }
}