using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Errors;
using Blauhaus.Auth.Abstractions.Extensions;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Commands;
using Blauhaus.Domain.Server.Actors;
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

        protected readonly ITimeService TimeService;
        protected TDbContext GetDbContext => _dbContextFactory.Invoke();

        protected BaseDbModelActor(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) : base(analyticsService)
        {
            _dbContextFactory = dbContextFactory;
            TimeService = timeService;
        }

        
        protected async Task<Response> HandleCommandAsync<TCommand>(TCommand command, IAuthenticatedUser user, Func<TDbContext, DateTime, Task<Response>> func)
        {
            return await TryExecuteAsync(async () =>
            {
                if (command is IAdminCommand)
                {
                    if (!user.IsAdminUser())
                    {
                        return AnalyticsService.TraceErrorResponse(this, AuthError.NotAuthorized);
                    }
                }

                using (var db = GetDbContext)
                {
                    var response = await func.Invoke(db, TimeService.CurrentUtcTime);
                    if (response.IsSuccess && db.ChangeTracker.HasChanges())
                    {
                        await db.SaveChangesAsync();
                    }

                    return response;
                }
            }, typeof(TCommand).Name, command.ToObjectDictionary());
        }


        protected async Task<Response<T>> HandleCommandAsync<T, TCommand>(TCommand command, IAuthenticatedUser user, Func<TDbContext, DateTime, Task<Response<T>>> func)
        {
            return await TryExecuteAsync(async () =>
            {
                if (command is IAdminCommand)
                {
                    if (!user.IsAdminUser())
                    {
                        return AnalyticsService.TraceErrorResponse<T>(this, AuthError.NotAuthorized);
                    }
                }

                using (var db = GetDbContext)
                {
                    var response = await func.Invoke(db, TimeService.CurrentUtcTime);
                    if (response.IsSuccess && db.ChangeTracker.HasChanges())
                    {
                        await db.SaveChangesAsync();
                    }

                    return response;
                }
            }, typeof(TCommand).Name, command.ToObjectDictionary());
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

        protected async Task<T> UpdateDbAsync<T>(Func<TDbContext, DateTime, Task<T>> func)
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