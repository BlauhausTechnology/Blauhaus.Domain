using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Server.Actors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.Actors
{
    public abstract class BaseDbModelActor<TDbContext, TModel> : BaseServerModelActor<TModel, Guid> 
        where TDbContext : DbContext
        where TModel : IHasId<Guid>
    {
        private readonly Func<TDbContext> _dbContextFactory;

        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ITimeService TimeService;
        protected TDbContext GetDbContext => _dbContextFactory.Invoke();

        protected BaseDbModelActor(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService)
        {
            _dbContextFactory = dbContextFactory;
            AnalyticsService = analyticsService;
            TimeService = timeService;
        }

        protected async Task<Response> TryExecuteCommandAsync<TCommand>(TCommand command, Func<TDbContext, Task<Response>> func)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand).Name} executed by {this.GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary()))
            {
                try
                {
                    using (var db = GetDbContext)
                    {
                        var response = await func.Invoke(db);
                        if (response.IsSuccess && db.ChangeTracker.HasChanges())
                        {
                            await db.SaveChangesAsync();
                        }
                        return response;
                    }
                }
                catch (Exception e)
                {
                    AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                    return Response.Failure(Error.Unexpected($"{typeof(TCommand).Name} failed to complete"));
                }
            }
        }
        
    }
}