using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Blauhaus.Domain.Server.EFCore.DtoLoaders
{
    public abstract class BaseDbDtoLoader<TDbContext, TEntity, TDto, TId> : BasePublisher, IEntityDtoLoader<TDto, TEntity, TId> 
        where TDbContext : DbContext
        where TEntity: class, IHasId<TId>
        where TId : IEquatable<TId> 
        where TDto : class, IHasId<TId>
    {
        
        private readonly Func<TDbContext> _dbContextFactory;
        protected TDbContext GetDbContext() =>
            _dbContextFactory.Invoke();

        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;

        protected BaseDbDtoLoader(
            IAnalyticsService analyticsService,
            ITimeService timeService,
            Func<TDbContext> dbContextFactory)
        {
            AnalyticsService = analyticsService;
            TimeService = timeService;
            _dbContextFactory = dbContextFactory;
        }

        public virtual async Task HandleAsync(TDto dto)
        {
            await UpdateSubscribersAsync(dto);
        }

        public virtual Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

        public async Task<TDto> GetOneAsync(TId id)
        {
            var dtos = await LoadDtosAsync(x => x.Id.Equals(id));

            return dtos.Count == 1 
                ? dtos[0] 
                : throw new ErrorException(DomainErrors.NotFound<TDto>());
        }

        public async Task<TDto?> TryGetOneAsync(TId id)
        {
            var dtos = await LoadDtosAsync(x => x.Id.Equals(id));

            return dtos.Count == 1 
                ? dtos[0] 
                : null;
        }

        public Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return LoadDtosAsync(x => true);
        }

        public Task<IReadOnlyList<TDto>> GetWhereAsync(Func<TEntity, bool> filter)
        {
            return LoadDtosAsync(x => filter.Invoke(x));
        }

        public Task<IReadOnlyList<TId>> GetIdsWhereAsync(Func<TEntity, bool> filter)
        {
            using var db = GetDbContext();

            return Task.FromResult<IReadOnlyList<TId>>(db.Set<TEntity>()
                .Where(filter).Select(x => x.Id).ToArray());
        }

        protected async Task<IReadOnlyList<TDto>> LoadDtosAsync(Expression<Func<TEntity, bool>> filter)
        {
            using var db = GetDbContext();
            var query = db.Set<TEntity>().Where(filter);
            query = Include(query);
            var entities = await query.ToListAsync();

            var dtoLoadTasks = new Task<TDto>[entities.Count];
            for (var i = 0; i < dtoLoadTasks.Length; i++)
            {
                dtoLoadTasks[i] = ExtractDtoAsync(entities[i]);
            }
            return await Task.WhenAll(dtoLoadTasks);
        }

        protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query)
        {
            return query;
        }

        protected abstract Task<TDto> ExtractDtoAsync(TEntity entity);

    }
}