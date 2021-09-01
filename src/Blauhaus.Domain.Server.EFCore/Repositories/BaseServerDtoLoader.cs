using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.Repositories
{
    public abstract class BaseServerDtoLoader<TDbContext, TDto, TEntity, TDtoId, TEntityId> : BasePublisher, IDtoCache<TDto, TDtoId> 
        where TDbContext : DbContext
        where TDto : class, IClientEntity<TDtoId>
        where TEntity : class, IServerEntity<TEntityId> 
        where TEntityId : IEquatable<TDtoId>
        where TDtoId : IEquatable<TDtoId>
    {
        private readonly Func<TDbContext> _dbContextFactory;
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;

        protected TDbContext GetDbContext() => 
            _dbContextFactory.Invoke();

        protected BaseServerDtoLoader(
            Func<TDbContext> dbContextFactory,
            IAnalyticsService analyticsService,
            ITimeService timeService)
        {
            _dbContextFactory = dbContextFactory;
            AnalyticsService = analyticsService;
            TimeService = timeService;
        }
         
        public async Task<TDto> GetOneAsync(TDtoId id)
        {
            using var db = GetDbContext();

            var entity = await db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
            {
                throw new ErrorException(DomainErrors.NotFound<TDto>());
            }

            return await PopulateDtoAsync(entity);
        }

        public async Task<TDto?> TryGetOneAsync(TDtoId id)
        {
            using var db = GetDbContext();

            var entity = await db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
            {
                return null;
            }

            return await PopulateDtoAsync(entity);
        }

        public async Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return await LoadEntitiesAsync(x => true); 
        }

        protected async Task<IReadOnlyList<TDto>> LoadEntitiesAsync(Expression<Func<TEntity, bool>> filter)
        {
            using var db = GetDbContext();
            
            var entities = await db.Set<TEntity>().Where(filter)
                .ToArrayAsync();
            
            var tasks = new Task<TDto>[entities.Length];
            for (var i = 0; i < entities.Length; i++)
            {
                tasks[i] = PopulateDtoAsync(entities[i]);
            }

            await Task.WhenAll(tasks);

            return tasks.Select(x => x.Result).ToArray();
        }
        
        public virtual Task HandleAsync(TDto dto)
        {
            return Task.CompletedTask;
        }


        public virtual Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            return Task.FromResult(base.AddSubscriber(handler, filter));
        }

         
        protected abstract Task<TDto> PopulateDtoAsync(TEntity entity);

    }
}