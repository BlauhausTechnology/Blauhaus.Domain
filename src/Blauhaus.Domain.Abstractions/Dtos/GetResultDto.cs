using System.Collections.Generic;

namespace Blauhaus.Domain.Abstractions.Dtos
{
    public class GetResultDto<TDto>
    {
        public List<TDto> CurrentBatch { get; set; } = new List<TDto>();
        public int TotalCount { get; set; }
        public int TotalPageCount { get; set; }
        public int CurrentPage { get; set; }
    }
}