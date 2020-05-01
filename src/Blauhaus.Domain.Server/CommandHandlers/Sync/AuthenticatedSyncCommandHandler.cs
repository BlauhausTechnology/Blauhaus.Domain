using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
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

            var dbQueryResult = await _queryLoader.HandleAsync(command, authenticatedUser, token);
            var dbQuery = dbQueryResult.Value.OrderByDescending(x => x.ModifiedAt).AsQueryable();

            var count = dbQuery.Count();


            if (command.ModifiedAfterTicks != null && command.ModifiedBeforeTicks != 0)
            {
                //new sync must begin with both specified
                var modifiedAfter = DateTime.SpecifyKind(new DateTime(command.ModifiedAfterTicks.Value), DateTimeKind.Utc);
                var modifiedBefore= DateTime.SpecifyKind(new DateTime(command.ModifiedBeforeTicks), DateTimeKind.Utc);
                dbQuery = dbQuery.Where(x => x.ModifiedAt> modifiedAfter || x.ModifiedAt < modifiedBefore);
            }

            else
            {
                //subsequent requests during sync only request ModifiedBefore
                if (command.ModifiedBeforeTicks != 0)
                {
                    var modifiedBefore= DateTime.SpecifyKind(new DateTime(command.ModifiedBeforeTicks), DateTimeKind.Utc);
                    dbQuery = dbQuery.Where(x => x.ModifiedAt < modifiedBefore);
                }

                //this should probably not be used
                if (command.ModifiedAfterTicks != null)
                {
                    var modifiedAfter = DateTime.SpecifyKind(new DateTime(command.ModifiedAfterTicks.Value), DateTimeKind.Utc);
                    dbQuery = dbQuery.Where(x => x.ModifiedAt > modifiedAfter);
                }
            }
            
            var entities = dbQuery
                .Take(command.BatchSize)
                .ToList();

            return Result.Success(new SyncResult<TEntity>
            {
                Entities = entities,
                TotalCount = count
            });
        }
    }
}