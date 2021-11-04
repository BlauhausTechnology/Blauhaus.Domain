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
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Errors;
using Blauhaus.Errors.Extensions;
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

        
        protected async Task<Response> TryExecuteCommandAsync<TCommand>(TCommand command, IAuthenticatedUser user, Func<TDbContext, DateTime, Task<Response>> func)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand).Name} executed by {this.GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary()))
            {
                try
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
                }
                catch (Exception e)
                {
                    if (e.IsErrorException())
                    {
                        return AnalyticsService.TraceErrorResponse(this, e.ToError());
                    }
                    AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                    return Response.Failure(Error.Unexpected($"{typeof(TCommand).Name} failed to complete"));
                }
            }
        } 

        protected async Task<Response<T>> TryExecuteCommandAsync<T, TCommand>(TCommand command, IAuthenticatedUser user, Func<TDbContext, DateTime, Task<Response<T>>> func)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{typeof(TCommand).Name} executed by {this.GetType().Name}", LogSeverity.Verbose, command.ToObjectDictionary()))
            {
                try
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
                }
                catch (Exception e)
                {
                    if (e.IsErrorException())
                    {
                        return AnalyticsService.TraceErrorResponse<T>(this, e.ToError());
                    }
                    AnalyticsService.LogException(this, e, command.ToObjectDictionary());
                    return Response.Failure<T>(Error.Unexpected($"{typeof(TCommand).Name} failed to complete"));
                }
            }
        } 
        
    }
}