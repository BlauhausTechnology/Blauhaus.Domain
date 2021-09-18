using System;
using System.Threading.Tasks; 
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{

    public interface ISyncDtoCache
    {
        Task<long> LoadLastModifiedAsync();
    }

    public interface ISyncDtoCache<TDto, in TId> : IDtoCache<TDto, TId>, ISyncDtoCache
        where TDto : class, IClientEntity<TId> where TId : IEquatable<TId>
    {
    }
}