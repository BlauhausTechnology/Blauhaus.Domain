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
        }

        public int RemainingDtoCount { get; }
        public IReadOnlyList<TDto> Dtos { get; }

        public int CurrentDtoCount => Dtos.Count;
        public long BatchLastModifiedTicks => Dtos.Max(x => x.ModifiedAtTicks);
    }
}