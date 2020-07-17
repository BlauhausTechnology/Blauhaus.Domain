using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Results;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Common.Errors;
using Blauhaus.Domain.Common.Extensions;
using CSharpFunctionalExtensions;

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

        public  async Task<Result<SyncResult<TEntity>>> HandleAsync(TSyncCommand command, TUser authenticatedUser, CancellationToken token)
        {

            if (command.IsForNewerEntities() && command.IsForOlderEntities())
            {
                return _analyticsService.TraceErrorResult<SyncResult<TEntity>>(this, SyncErrors.InvalidSyncCommand);
            }

            var dbQueryResult = await _queryLoader.HandleAsync(command, authenticatedUser, token);
            if (dbQueryResult.IsFailure)
            {
                return Result.Failure<SyncResult<TEntity>>(dbQueryResult.Error);
            }

            var dbQuery = dbQueryResult.Value;
            var traceMessage = string.Empty;

            var totalActiveEntityCount = dbQuery.Count(x => x.EntityState != EntityState.Deleted);

            if(command.IsForSingleEntity())
            {
                dbQuery = dbQuery.Where(x => x.Id == command.Id.Value);

                if (command.IsForNewerEntities())
                {
                    dbQuery = dbQuery.Where(x => x.ModifiedAt > command.NewerThan.ToUtcDateTime());
                }

                traceMessage = "SyncCommand for single entity processed";
            }

            else if (command.IsFirstSyncForDevice())
            {
                dbQuery = dbQuery.Where(x => x.EntityState != EntityState.Deleted)
                    .OrderByDescending(x => x.ModifiedAt);
                traceMessage = "SyncCommand for new device processed";
            }

            else if (command.IsForOlderEntities())
            {
                //we are returning entities that don't exist on device so we can ignore deleted entities
                dbQuery = dbQuery.Where(x => 
                    x.EntityState != EntityState.Deleted && 
                    x.ModifiedAt < command.OlderThan.ToUtcDateTime())
                    .OrderByDescending(x => x.ModifiedAt);
                traceMessage = "SyncCommand for older entities processed";
            }
             
            else if (command.IsForNewerEntities())
            {
                dbQuery = dbQuery.Where(x => x.ModifiedAt > command.NewerThan.ToUtcDateTime())
                    .OrderBy(x => x.ModifiedAt);
                traceMessage = "SyncCommand for newer entities processed";
            }
             
            var modifiedEntityCount = dbQuery.Count();
            
            var entities = dbQuery
                .Take(command.BatchSize)
                .ToList();

            _analyticsService.TraceVerbose(this, traceMessage, new Dictionary<string, object>
            {
                {nameof(SyncResult<TEntity>.EntityBatch) + "Count", entities.Count},
                {nameof(SyncResult<TEntity>.TotalActiveEntityCount), totalActiveEntityCount},
                {nameof(SyncResult<TEntity>.EntitiesToDownloadCount), modifiedEntityCount}
            });

            return Result.Success(new SyncResult<TEntity>
            {
                EntityBatch = entities,
                EntitiesToDownloadCount = modifiedEntityCount,
                TotalActiveEntityCount = totalActiveEntityCount
            });

             
        }
    }
}