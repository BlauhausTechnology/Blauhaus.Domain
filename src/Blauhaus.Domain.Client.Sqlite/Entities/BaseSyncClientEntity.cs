using System;
using Blauhaus.Domain.Common.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Entities
{
    public abstract class BaseSyncClientEntity : BaseClientEntity, ISyncClientEntity
    {
        [Indexed]
        public SyncState SyncState { get; set; }
    }
}