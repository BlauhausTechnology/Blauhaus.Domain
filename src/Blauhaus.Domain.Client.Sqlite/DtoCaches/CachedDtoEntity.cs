using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Newtonsoft.Json;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.DtoCaches
{
    public class CachedDtoEntity<TId> : ClientEntity<TId>, ISyncClientEntity<TId>
    {
         
        [Indexed]
        public SyncState SyncState { get; set; }

        public string SerializedDto { get; set; } = string.Empty;

      
         
    }
}