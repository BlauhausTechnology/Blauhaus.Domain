using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.DtoLoaders
{

    public abstract class BaseDbEntityDtoLoader<TDbContext, TEntity, TDto, TId> : BaseDbDtoLoader<TDbContext, TEntity, TDto, TId>
        where TDbContext : DbContext
        where TEntity : class, IHasId<TId>, IDtoOwner<TDto>
        where TId : IEquatable<TId>
        where TDto : class, IHasId<TId>
    {
        protected BaseDbEntityDtoLoader(
            IAnalyticsLogger logger, 
            ITimeService timeService, 
            Func<TDbContext> dbContextFactory) 
                : base(logger, timeService, dbContextFactory)
        {
        }

        protected override Task<TDto> ExtractDtoAsync(TEntity entity)
        {
            return entity.GetDtoAsync();
        }
    }

    
}