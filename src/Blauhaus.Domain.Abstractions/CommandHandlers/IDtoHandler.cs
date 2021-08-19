using Blauhaus.Common.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IDtoHandler<in TDto>
    {
        Task HandleAsync(TDto dto);
    }

    public interface IDtoCache<TDto, in TId> : IAsyncPublisher<TDto>, IDtoHandler<TDto>
        where TDto : class, IHasId<TId>
    {
        Task<TDto> GetOneAsync(TId id);
        Task<TDto?> TryGetOneAsync(TId id);
        Task<IReadOnlyList<TDto>> GetAllAsync();
        Task<IReadOnlyList<TDto>> GetWhereAsync(Func<TDto, bool> search);
    }

}