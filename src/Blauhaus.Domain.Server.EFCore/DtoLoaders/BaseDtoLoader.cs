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

//namespace Blauhaus.Domain.Server.EFCore.DtoLoaders
//{
//    public abstract class BaseDbDtoLoader<TDbContext, TEntity, TDto, TId> : BasePublisher, IDtoLoader<TDto, TId>
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
//            return Task.FromResult(base.AddSubscriber(handler, filter));
//        }

//        public Task<TDto> GetOneAsync(TId id)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<TDto?> TryGetOneAsync(TId id)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IReadOnlyList<TDto>> GetAllAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}