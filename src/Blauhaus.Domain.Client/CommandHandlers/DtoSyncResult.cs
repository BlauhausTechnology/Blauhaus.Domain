using System.Collections.Generic;

namespace Blauhaus.Domain.Client.CommandHandlers
{
    public class DtoSyncResult<TModelDto>
    {
        public IReadOnlyList<TModelDto> Dtos { get; set; } = new List<TModelDto>();
        public long TotalEntityCount { get; set; }
        public long ModifiedEntityCount { get; set; }
    }
}