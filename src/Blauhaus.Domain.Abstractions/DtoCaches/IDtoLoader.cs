using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface IDtoLoader<TDto, in TId> : IAsyncPublisher<TDto>
        where TDto : class, IHasId<TId>
        where TId : IEquatable<TId>
    {
        Task<TDto> GetOneAsync(TId id);
        Task<TDto?> TryGetOneAsync(TId id);
        Task<IReadOnlyList<TDto>> GetAllAsync(); 
    }
}