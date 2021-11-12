using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;

namespace Blauhaus.Domain.Server.EFCore.Actors
{
    public abstract class BaseCollectionActor<TDbContext, TModel> : BaseAsyncCollectionPublisher<TModel>
        where TDbContext : DbContext
    {
        private readonly Func<TDbContext> _dbContextFactory;
        protected TDbContext GetDbContext => _dbContextFactory.Invoke();

        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;

        protected BaseCollectionActor(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
        {
            _dbContextFactory = dbContextFactory;
            AnalyticsService = analyticsService;
            TimeService = timeService;
        }
    }
}