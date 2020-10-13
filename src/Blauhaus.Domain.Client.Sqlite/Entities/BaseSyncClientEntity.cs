using System;
using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Entities
{
    public abstract class BaseSyncClientEntity : BaseClientEntity, ISyncClientEntity
    {
        [Indexed]
        public SyncState SyncState { get; set; }
    }
}