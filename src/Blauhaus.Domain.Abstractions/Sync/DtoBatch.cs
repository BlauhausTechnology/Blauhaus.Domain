using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Abstractions.Entities;
using Newtonsoft.Json;

namespace Blauhaus.Domain.Abstractions.Sync
{
    public class DtoBatch<TDto, TId> 
        where TDto :  IClientEntity<TId> 
        where TId : IEquatable<TId>
    {
   
        [JsonConstructor]
        public DtoBatch(
            IReadOnlyList<TDto> dtos, 
            int remainingDtoCount)
        {
            Dtos = dtos;
            RemainingDtoCount = remainingDtoCount;
            BatchLastModifiedTicks = Dtos.Count == 0 ? 0 : Dtos.Max(x => x.ModifiedAtTicks);
            CurrentDtoCount = Dtos.Count;
        }

        public int RemainingDtoCount { get; }
        public IReadOnlyList<TDto> Dtos { get; }

        public int CurrentDtoCount { get; }
        public long BatchLastModifiedTicks { get; }

        public static DtoBatch<TDto, TId> Empty()
        {
            return new DtoBatch<TDto, TId>(Array.Empty<TDto>(), 0);
        }
    }
}