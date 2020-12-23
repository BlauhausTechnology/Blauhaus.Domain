using System.Collections.Generic;

namespace Blauhaus.Domain.Abstractions.Sync
{
    public class SyncResult<TPayload>  
    {
        public List<TPayload> EntityBatch { get; set; } = new List<TPayload>();
        public long TotalActiveEntityCount { get; set; }
        public long EntitiesToDownloadCount { get; set; }
    }
}