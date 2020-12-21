using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Domain.Abstractions.Extensions;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Server.CommandHandlers.Sync
{
    public class AuthenticatedSyncCommandHandler<TEntity, TSyncCommand, TUser> : IAuthenticatedCommandHandler<SyncResult<TEntity>, TSyncCommand, TUser>
        where TEntity : IServerEntity
        where TSyncCommand : SyncCommand
        where TUser : notnull
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IAuthenticatedSyncQueryLoader<TEntity, TSyncCommand, TUser> _queryLoader;

        public AuthenticatedSyncCommandHandler(
            IAnalyticsService analyticsService,
            IAuthenticatedSyncQueryLoader<TEntity, TSyncCommand, TUser> queryLoader)
        {
            _analyticsService = analyticsService;
            _queryLoader = queryLoader;
        }

        public  async Task<Response<SyncResult<TEntity>>> HandleAsync(TSyncCommand command, TUser authenticatedUser)
        {

            if (command.IsForNewerEntities() && command.IsForOlderEntities())
            {
                return _analyticsService.TraceErrorResponse<SyncResult<TEntity>>(this, SyncErrors.InvalidSyncCommand);
            }

            var dbQueryResult = await _queryLoader.HandleAsync(command, authenticatedUser);
            if (dbQueryResult.IsFailure)
            {
                return Response.Failure<SyncResult<TEntity>>(dbQueryResult.Error);
            }

            var dbQuery = dbQueryResult.Value;
            var traceMessage = string.Empty;

            var totalActiveEntityCount = dbQuery.Count(x => x.EntityState != EntityState.Deleted);

            var entityName = typeof(TEntity).Name;

            if(command.IsForSingleEntity())
            {
                dbQuery = dbQuery.Where(x => x.Id == command.Id.Value);

                if (command.IsForNewerEntities())
                {
                    dbQuery = dbQuery.Where(x => x.ModifiedAt > command.NewerThan.ToUtcDateTime());
                }

                traceMessage = $"SyncCommand for single {entityName} entity processed";
            }

            else if (command.IsFirstSyncForDevice())
            {
                dbQuery = dbQuery.Where(x => x.EntityState != EntityState.Deleted)
                    .OrderByDescending(x => x.ModifiedAt);
                traceMessage = $"SyncCommand for all {entityName} entities processed";
            }

            else if (command.IsForOlderEntities())
            {
                //we are returning entities that don't exist on device so we can ignore deleted entities
                dbQuery = dbQuery.Where(x => 
                    x.EntityState != EntityState.Deleted && 
                    x.ModifiedAt < command.OlderThan.ToUtcDateTime())
                    .OrderByDescending(x => x.ModifiedAt);
                traceMessage = $"SyncCommand for older {entityName} entities processed";
            }
             
            else if (command.IsForNewerEntities())
            {
                dbQuery = dbQuery.Where(x => x.ModifiedAt > command.NewerThan.ToUtcDateTime())
                    .OrderBy(x => x.ModifiedAt);
                traceMessage = $"SyncCommand for newer {entityName} entities processed";
            }
             
            var modifiedEntityCount = dbQuery.Count();
            
            var entities = dbQuery
                .Take(command.BatchSize)
                .ToList();

            traceMessage += $". {entities.Count} returned. {modifiedEntityCount - entities.Count} still to send out of {totalActiveEntityCount} in total";

            _analyticsService.TraceVerbose(this, traceMessage);

            return Response.Success(new SyncResult<TEntity>
            {
                EntityBatch = entities,
                EntitiesToDownloadCount = modifiedEntityCount,
                TotalActiveEntityCount = totalActiveEntityCount
            });

             
        }
    }
}