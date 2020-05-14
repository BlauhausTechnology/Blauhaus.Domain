using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ISyncQueryLoader<TRootEntity, TSyncCommand> _syncQueryGenerator;
        protected Query CreateSqlQuery(TSyncCommand syncCommand) => _syncQueryGenerator.GenerateQuery(syncCommand);
         
        public BaseSyncClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityConverter<TModel, TDto, TRootEntity> entityConverter,
            ISyncQueryLoader<TRootEntity, TSyncCommand> syncQueryGenerator) 
                : base(analyticsService, sqliteDatabaseService, entityConverter)
        {
            _syncQueryGenerator = syncQueryGenerator;
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
                 

                var sql = SqlCompiler.Compile(query).ToString();
                var entities = connection.Query<TRootEntity>(sql);

                foreach (var entity in entities)
                {
                    models.Add(EntityConverter.ConstructModelFromRootEntity(entity, connection));
                }
            });

            return models;
        }

        public async Task<IReadOnlyList<TModel>> SaveSyncedDtosAsync(IEnumerable<TDto> dtos)
        {
            var models = new List<TModel>();
            var entities = new List<IClientEntity>();

            var db = await DatabaseService.GetDatabaseConnectionAsync();
            await db.RunInTransactionAsync(connection =>
            {
                foreach (var dto in dtos)
                {
                    var rootEntity = EntityConverter.ExtractRootEntityFromDto(dto);
                    rootEntity.SyncState = SyncState.InSync;
                    entities.Add(rootEntity);

                    var childEntities = EntityConverter.ExtractChildEntitiesFromDto(dto);
                    foreach (var childEntity in childEntities)
                    {
                        childEntity.SyncState = SyncState.InSync;
                        entities.Add(childEntity);
                    }

                    models.Add(EntityConverter.ConstructModelFromRootEntity(rootEntity, connection));
                }

                foreach (var entity in entities)
                {
                    connection.InsertOrReplace(entity);
                }
            });

            return models;
        }
    }
}