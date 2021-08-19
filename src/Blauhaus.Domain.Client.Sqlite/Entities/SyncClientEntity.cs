using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Entities
{
    public abstract class SyncClientEntity : ClientEntity, ISyncClientEntity
    {
        [Indexed]
        public SyncState SyncState { get; set; }
    }
}