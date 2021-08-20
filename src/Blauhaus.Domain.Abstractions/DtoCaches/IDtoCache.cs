using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface IDtoCache<TDto, in TId> : IAsyncPublisher<TDto>, IDtoHandler<TDto>
        where TDto : class, IHasId<TId>
    {
        Task<TDto> GetOneAsync(TId id);
        Task<TDto?> TryGetOneAsync(TId id);
        Task<IReadOnlyList<TDto>> GetAllAsync(); 
    }

     
}