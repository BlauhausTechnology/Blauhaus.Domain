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
            int remainingDtoCount, 
            int currentDtoCount, 
            long batchLastModifiedTicks)
        {
            RemainingDtoCount = remainingDtoCount;
            Dtos = dtos;
            CurrentDtoCount = currentDtoCount;
            BatchLastModifiedTicks = batchLastModifiedTicks;
        }


        public int RemainingDtoCount { get; }
        public IReadOnlyList<TDto> Dtos { get; }
        public int CurrentDtoCount { get; }
        public long BatchLastModifiedTicks { get; }

        public static DtoBatch<TDto, TId>Create(IReadOnlyList<TDto> dtos, int remainingDtoCount)
        {
            return new DtoBatch<TDto, TId>(
                dtos,
                remainingDtoCount,
                dtos.Count,
                dtos.Count == 0 ? 0 : dtos.Max(x => x.ModifiedAtTicks));
        }

        public static DtoBatch<TDto, TId> Empty()
        {
            return Create(Array.Empty<TDto>(), 0);
        }

        public override string ToString()
        {
            return $"{typeof(TDto).Name}: {CurrentDtoCount} / {CurrentDtoCount+RemainingDtoCount}";
        }
    }
}