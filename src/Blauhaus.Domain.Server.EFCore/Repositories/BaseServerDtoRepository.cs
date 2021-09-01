using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.Repositories
{
    public abstract class BaseServerDtoRepository<TDbContext, TDto, TId> : IDtoCache<TDto, TId> 
        where TDbContext : DbContext
        where TDto : class, IClientEntity<TId>
    {
        private readonly Func<TDbContext> _dbContextFactory;
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;

        protected TDbContext GetDbContext() => 
            _dbContextFactory.Invoke();

        protected BaseServerDtoRepository(
            Func<TDbContext> dbContextFactory,
            IAnalyticsService analyticsService,
            ITimeService timeService)
        {
            _dbContextFactory = dbContextFactory;
            AnalyticsService = analyticsService;
            TimeService = timeService;
        }

        public Task HandleAsync(TDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<TDto> GetOneAsync(TId id)
        {
            throw new NotImplementedException();
        }

        public Task<TDto?> TryGetOneAsync(TId id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}