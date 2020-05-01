using System.Collections.Generic;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Common.CommandHandlers.Sync
{
    public class SyncResult<TPayload>  
        where TPayload : IEntity
    {
        public List<TPayload> Entities { get; set; } = new List<TPayload>();
        public int TotalCount { get; set; }
    }
}