using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;

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