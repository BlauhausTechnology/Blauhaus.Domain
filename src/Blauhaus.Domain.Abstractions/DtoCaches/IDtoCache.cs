using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{

    public interface IDtoCache<TDto, in TId> : IDtoLoader<TDto, TId>, IDtoHandler<TDto>
        where TDto : class, IHasId<TId>
        where TId : IEquatable<TId>
    {
        Task DeleteOneAsync(TId id);
        Task DeleteAllAsync();
    }

     
}