using System;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface ISyncDtoCache<TDto, in TId> : IDtoCache<TDto, TId>
        where TDto : class, IClientEntity<TId> where TId : IEquatable<TId>
    {
        Task<long> LoadLastModifiedAsync();
    }
}