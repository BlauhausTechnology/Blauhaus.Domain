using System.Collections.Generic;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Common.CommandHandlers.Sync
{
    public class SyncResult<TPayload>  
        where TPayload : IEntity
    {
        public List<TPayload> Entities { get; set; } = new List<TPayload>();
        public long TotalEntityCount { get; set; }
        public long ModifiedEntityCount { get; set; }
    }
}