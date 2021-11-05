using System;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.Actors
{
    public abstract class BaseEntityModelActor<TDbContext, TEntity, TModel> : BaseDbModelActor<TDbContext, TModel> 
        where TDbContext : DbContext
        where TModel : IHasId<Guid> 
        where TEntity : BaseServerEntity
    {
        protected TEntity LoadedEntity => Entity ?? throw new InvalidOperationException("Entity does not exist");
        protected TEntity? Entity;

        protected BaseEntityModelActor(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
            : base(dbContextFactory, analyticsService, timeService)
        {
        }


        protected override async Task OnInitializedAsync(Guid id)
        {
            using (var db = GetDbContext)
            {
                var query = db.Set<TEntity>().Where(x => x.Id.Equals(id));
                query = Include(query);
                Entity = await query.FirstOrDefaultAsync();

                if (Entity != null)
                {
                    await HandleEntityLoadedAsync(db, Entity);
                }
            }
        }

        protected virtual Task HandleEntityLoadedAsync(TDbContext db, TEntity entity)
        {
            return Task.CompletedTask;
        }

        protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query)
        {
            return query;
        }
    }
}