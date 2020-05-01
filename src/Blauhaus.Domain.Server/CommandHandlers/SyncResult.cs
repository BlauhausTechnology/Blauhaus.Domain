using System.Collections.Generic;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Server.CommandHandlers
{
    public class SyncResult<TPayload>  
        where TPayload : IServerEntity
    {
        public List<TPayload> Entities { get; set; } = new List<TPayload>();
        public int TotalCount { get; set; }
    }
}