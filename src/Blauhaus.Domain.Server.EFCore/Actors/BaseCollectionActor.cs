using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Responses;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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

        protected async Task<Response<T>> UpdateDbAsync<T>(Func<TDbContext, DateTime, Task<Response<T>>> func)
        {
            using (var db = GetDbContext)
            {
                var response = await func.Invoke(db, TimeService.CurrentUtcTime);
                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }

                return response;
            }
        }

        protected async Task<Response> UpdateDbAsync(Func<TDbContext, DateTime, Task<Response>> func)
        {
            using (var db = GetDbContext)
            {
                var response = await func.Invoke(db, TimeService.CurrentUtcTime);
                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }

                return response;
            }
        }
    }
}