using Blauhaus.Common.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface IEntityDtoLoader<TDto, out TEntity, TId> : IDtoLoader<TDto, TId>
        where TDto : class, IHasId<TId>
        where TEntity : IHasId<TId>
        where TId : IEquatable<TId>
    {
        Task<IReadOnlyList<TDto>> GetWhereAsync(Func<TEntity, bool> filter);
        Task<IReadOnlyList<TId>> GetIdsWhereAsync(Func<TEntity, bool> filter);
    }
}