using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class BaseSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity> : BaseClientRepository<TModel,TDto,TRootEntity>, ISyncClientRepository<TModel,TDto, TSyncCommand> 
        where TRootEntity : BaseSyncClientEntity, new() 
        where TModel : class, IClientEntity 
        where TSyncCommand : SyncCommand
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISyncQueryLoader<TSyncCommand, TRootEntity> _syncQueryLoader;
        protected Query CreateSqlQuery(TSyncCommand syncCommand) => _syncQueryLoader.GenerateQuery(syncCommand);
         
        public BaseSyncClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityConverter<TModel, TDto, TRootEntity> entityConverter,
            ISyncQueryLoader<TSyncCommand, TRootEntity> syncQueryLoader) 
                : base(analyticsService, sqliteDatabaseService, entityConverter)
        {
            _analyticsService = analyticsService;
            _syncQueryLoader = syncQueryLoader;
        }
        


        public async Task<ClientSyncStatus> GetSyncStatusAsync(TSyncCommand syncCommand)
        {
            var syncStatus = new ClientSyncStatus();
            var db = await DatabaseService.GetDatabaseConnectionAsync();
            await db.RunInTransactionAsync(connection =>
            {

                var newestModifiedQuery = CreateSqlQuery(syncCommand)
                    .Where(nameof(ISyncClientEntity.SyncState), "=", SyncState.InSync)
                    .OrderByDesc(nameof(IClientEntity.ModifiedAtTicks))
                    .Take(1);
                var newestModifiedSql = SqlCompiler.Compile(newestModifiedQuery).ToString();
                var newestModified = connection.Query<TRootEntity>(newestModifiedSql);

                var oldesModifiedQuery =  CreateSqlQuery(syncCommand)
                    .Where(nameof(ISyncClientEntity.SyncState), "=", SyncState.InSync)
                    .OrderBy(nameof(IClientEntity.ModifiedAtTicks))
                    .Take(1);
                var oldestModifiedSql = SqlCompiler.Compile(oldesModifiedQuery).ToString();
                var oldestModified = connection.Query<TRootEntity>(oldestModifiedSql);

                var syncedCountQuery =  CreateSqlQuery(syncCommand)
                    .Where(nameof(ISyncClientEntity.SyncState), "=", SyncState.InSync).AsCount();
                var syncedCountSql = SqlCompiler.Compile(syncedCountQuery).ToString();
                var syncedCount = connection.ExecuteScalar<long>(syncedCountSql);

                var allCountQuery = CreateSqlQuery(syncCommand);
                var allCountSql = SqlCompiler.Compile(allCountQuery.AsCount()).ToString();
                var allCount = connection.ExecuteScalar<long>(allCountSql);
                
                syncStatus.NewestModifiedAt = newestModified.FirstOrDefault()?.ModifiedAtTicks;
                syncStatus.OldestModifiedAt = oldestModified.LastOrDefault()?.ModifiedAtTicks ?? 0;
                syncStatus.SyncedLocalEntities = syncedCount;
                syncStatus.AllLocalEntities = allCount;

                _analyticsService.TraceVerbose(this, "SyncStatus loaded", syncStatus.ToObjectDictionary()
                    .WithValue("Newest query", newestModifiedSql)
                    .WithValue("Oldest query", oldestModifiedSql)
                    .WithValue("Synced count query", syncedCountSql)
                    .WithValue("All count query", allCountSql));

            });

            return syncStatus;
        }

        public async Task<IReadOnlyList<TModel>> LoadModelsAsync(TSyncCommand syncCommand)
        { 
            var db = await DatabaseService.GetDatabaseConnectionAsync();
            var models = new List<TModel>();

            await db.RunInTransactionAsync(connection =>
            {
                var query = CreateSqlQuery(syncCommand)
                    .OrderByDesc(nameof(IClientEntity.ModifiedAtTicks))
                    .Take(syncCommand.BatchSize);

                if (syncCommand.OlderThan > 0)
                {
                    query = query.Where(nameof(IClientEntity.ModifiedAtTicks), "<", syncCommand.OlderThan);
                }
                else if (syncCommand.NewerThan > 0)
                {
                    query = query.Where(nameof(IClientEntity.ModifiedAtTicks), ">", syncCommand.NewerThan);
                }

                var sql = SqlCompiler.Compile(query).ToString();
                var rootEntities = connection.Query<TRootEntity>(sql);

                foreach (var rootEntity in rootEntities)
                {
                    var childEntities = EntityConverter.LoadChildEntities(rootEntity, connection);
                    models.Add(EntityConverter.ConstructModel(rootEntity, childEntities));
                }
                
                _analyticsService.TraceVerbose(this, "Models loaded", new Dictionary<string, object>
                    {
                        {"Count", models.Count},
                        {"SQL query", sql}
                    });

            });

            return models;
        }

        public async Task<IReadOnlyList<TModel>> SaveSyncedDtosAsync(IEnumerable<TDto> dtos)
        {
            var models = new List<TModel>();
            var allEntities = new List<ISyncClientEntity>();

            var db = await DatabaseService.GetDatabaseConnectionAsync();
            await db.RunInTransactionAsync(connection =>
            {
                foreach (var dto in dtos)
                {
                    var entities = EntityConverter.ExtractEntitiesFromDto(dto);

                    var rootEntity = entities.Item1;
                    var childEntities = entities.Item2;

                    allEntities.Add(rootEntity);
                    allEntities.AddRange(childEntities);
                    models.Add(EntityConverter.ConstructModel(rootEntity, childEntities));
                }

                foreach (var entity in allEntities)
                {
                    entity.SyncState = SyncState.InSync;
                    connection.InsertOrReplace(entity);
                }
            });

            return models;
        }
    }
}