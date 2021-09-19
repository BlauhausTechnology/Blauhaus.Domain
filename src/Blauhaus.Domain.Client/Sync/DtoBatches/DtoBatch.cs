using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Newtonsoft.Json;

namespace Blauhaus.Domain.Client.Sync.DtoBatches
{
    public class DtoBatch<TDto, TId> : IDtoBatch<TDto> 
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