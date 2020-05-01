using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Server.Entities;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Server.CommandHandlers
{
    public class AuthenticatedSyncQueryHandler<TEntity, TSyncQuery, TUser> : IAuthenticatedCommandHandler<SyncResult<TEntity>, TSyncQuery, TUser>
        where TEntity : IServerEntity
        where TSyncQuery : SyncCommand
        where TUser : notnull
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IAuthenticatedSyncQueryLoader<TEntity, TSyncQuery, TUser> _queryLoader;

        public AuthenticatedSyncQueryHandler(
            IAnalyticsService analyticsService,
            IAuthenticatedSyncQueryLoader<TEntity, TSyncQuery, TUser> queryLoader)
        {
            _analyticsService = analyticsService;
            _queryLoader = queryLoader;
        }

        public  async Task<Result<SyncResult<TEntity>>> HandleAsync(TSyncQuery query, TUser authenticatedUser, CancellationToken token)
        {

            var dbQueryResult = await _queryLoader.HandleAsync(query, authenticatedUser, token);
            var dbQuery = dbQueryResult.Value.OrderByDescending(x => x.ModifiedAt).AsQueryable();

            var count = dbQuery.Count();


            if (query.ModifiedAfter != null && query.ModifiedBefore != null)
            {
                //new sync must begin with both specified
                var modifiedAfter = DateTime.SpecifyKind(new DateTime(query.ModifiedAfter.Value), DateTimeKind.Utc);
                var modifiedBefore = DateTime.SpecifyKind(new DateTime(query.ModifiedBefore.Value), DateTimeKind.Utc);
                dbQuery = dbQuery.Where(x => x.ModifiedAt > modifiedAfter || x.ModifiedAt < modifiedBefore);
            }

            else
            {
                //subsequent requests during sync only request ModifiedBefore
                if (query.ModifiedBefore != null)
                {
                    var modifiedBefore = DateTime.SpecifyKind(new DateTime(query.ModifiedBefore.Value), DateTimeKind.Utc);
                    dbQuery = dbQuery.Where(x => x.ModifiedAt < modifiedBefore);
                }

                //this should probably not be used
                if (query.ModifiedAfter != null)
                {
                    var modifiedAfter = DateTime.SpecifyKind(new DateTime(query.ModifiedAfter.Value), DateTimeKind.Utc);
                    dbQuery = dbQuery.Where(x => x.ModifiedAt > modifiedAfter);
                }
            }
            
            var entities = dbQuery
                .Take(query.BatchSize)
                .ToList();

            return Result.Success(new SyncResult<TEntity>
            {
                Entities = entities,
                TotalCount = count
            });
        }
    }
}