using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface IDtoLoader<TDto, TId> : IAsyncPublisher<TDto>
        where TDto : class, IHasId<TId>
        where TId : IEquatable<TId>
    {
        Task<TDto> GetOneAsync(TId id);
        Task<TDto?> TryGetOneAsync(TId id);
        Task<IReadOnlyList<TDto>> GetAllAsync();
        Task<IReadOnlyList<TDto>> GetWhereAsync(Func<TDto, bool> filter);
        Task<IReadOnlyList<TId>> GetIdsWhereAsync(Func<TDto, bool> filter);
    }


    public interface IDtoCache<TDto, TId> : IDtoLoader<TDto, TId>, IDtoHandler<TDto>
        where TDto : class, IHasId<TId>
        where TId : IEquatable<TId>
    {
        Task DeleteOneAsync(TId id);
        Task DeleteAllAsync();
    }

     
}