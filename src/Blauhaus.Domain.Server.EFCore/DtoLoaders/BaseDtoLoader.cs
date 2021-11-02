//using Blauhaus.Analytics.Abstractions.Service;
//using Blauhaus.Domain.Abstractions.Entities;
//using Blauhaus.Time.Abstractions;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Blauhaus.Common.Utils.Disposables;
//using Blauhaus.Domain.Abstractions.DtoCaches;
//using Blauhaus.Domain.Abstractions.DtoHandlers;
//using Blauhaus.Domain.Server.Entities;
//using System.Linq.Expressions;
//using System.Linq;

//namespace Blauhaus.Domain.Server.EFCore.DtoLoaders
//{
//    public abstract class BaseDbDtoLoader<TDbContext, TEntity, TDto, TId> : BasePublisher, IDtoCache<TDto, TId>
//        where TDbContext : DbContext
//        where TDto : class, IClientEntity<TId>
//        where TId : IEquatable<TId>
//        where TEntity : BaseServerEntity, IDtoOwner<TDto>
//    {

//        private readonly Func<TDbContext> _dbContextFactory;
//        protected TDbContext GetDbContext() =>
//            _dbContextFactory.Invoke();

//        protected BaseDbDtoLoader(
//            IAnalyticsService analyticsService,
//            ITimeService timeService,
//            Func<TDbContext> dbContextFactory)
//        {
//            _dbContextFactory = dbContextFactory;
//        }

//        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
//        {
//            return Task.FromResult(AddSubscriber(handler, filter));
//        }

//        public virtual async Task<TDto> GetOneAsync(TId id)
//        {
//            using (var db = GetDbContext())
//            {
//                var entity = await db.Set<TEntity>()
//                    .FirstAsync(x => x.Id.Equals(id));
//                return await entity.GetDtoAsync();
//            }
//        }

//        public virtual async Task<TDto?> TryGetOneAsync(TId id)
//        {

//            using (var db = GetDbContext())
//            {
//                var entity = await db.Set<TEntity>()
//                    .FirstOrDefaultAsync(x => x.Id.Equals(id));
//                if (entity == null) return null;
//                return await entity.GetDtoAsync();
//            }
//        }

//        public virtual async Task<IReadOnlyList<TDto>> GetAllAsync()
//        {
//            return await LoadDtosAsync(x => true);
//        }

//        protected virtual Task<IReadOnlyList<TDto>> LoadDtosAsync(Expression<Func<TEntity, bool>> filter)
//        {
//            return LoadDtosFromDatabaseAsync(filter);
//        }

//        protected async Task<IReadOnlyList<TDto>> LoadDtosFromDatabaseAsync(Expression<Func<TEntity, bool>> filter)
//        {
//            using var db = GetDbContext();

//            var entities = await db.Set<TEntity>()
//                .AsNoTracking()
//                .Where(filter)
//                .ToListAsync();

//            var dtos = new TDto[entities.Count];

//            for (var i = 0; i < entities.Count; i++)
//            {
//                dtos[i] = await entities[i].GetDtoAsync();
//            }

//            return dtos;
//        }
//    }
//}