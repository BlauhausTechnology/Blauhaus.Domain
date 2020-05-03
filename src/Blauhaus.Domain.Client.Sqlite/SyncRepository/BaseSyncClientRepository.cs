using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Entities;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class BaseSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity> : BaseClientRepository<TModel,TDto,TRootEntity>, ISyncClientRepository<TModel,TDto, TSyncCommand> 
        where TRootEntity : BaseSqliteEntity, new() 
        where TModel : class, IClientEntity 
        where TSyncCommand : SyncCommand
    {
        private readonly ISyncQueryGenerator<TSyncCommand> _syncQueryGenerator;

        public BaseSyncClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityManager<TModel, TDto, TRootEntity> entityManager,
            ISyncQueryGenerator<TSyncCommand> syncQueryGenerator) 
                : base(analyticsService, sqliteDatabaseService, entityManager)
        {
            _syncQueryGenerator = syncQueryGenerator;
        }

        public async Task<ClientSyncStatus> GetSyncStatusAsync()
        {
            var syncStatus = new ClientSyncStatus();
            var db = await DatabaseService.GetDatabaseConnectionAsync();
            await db.RunInTransactionAsync(connection =>
            {
                var orderedQuery = connection.Table<TRootEntity>().OrderByDescending(x => x.ModifiedAtTicks);
                syncStatus.LastModifiedAt = orderedQuery.FirstOrDefault()?.ModifiedAtTicks;
                syncStatus.FirstModifiedAt = orderedQuery.LastOrDefault()?.ModifiedAtTicks ?? 0;
                syncStatus.TotalCount = orderedQuery.Count();
            });

            return syncStatus;
        }

        public async Task<IReadOnlyList<TModel>> LoadSyncedModelsAsync(TSyncCommand syncCommand)
        { 
            var db = await DatabaseService.GetDatabaseConnectionAsync();
            var models = new List<TModel>();

            await db.RunInTransactionAsync(connection =>
            {
                var query = SqlQuery()
                    .OrderByDesc(nameof(IClientEntity.ModifiedAtTicks))
                    .Take(syncCommand.BatchSize);

                if (syncCommand.ModifiedBeforeTicks > 0)
                {
                    query = query.Where(nameof(IClientEntity.ModifiedAtTicks), "<", syncCommand.ModifiedBeforeTicks);
                }

                query = _syncQueryGenerator.ExtendQuery(query, syncCommand);

                var sql = SqlCompiler.Compile(query).ToString();
                var entities = connection.Query<TRootEntity>(sql);

                foreach (var entity in entities)
                {
                    models.Add(EntityManager.ConstructModelFromRootEntity(entity, connection));
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
                    var rootEntity = EntityManager.ExtractRootEntityFromDto(dto);
                    entities.Add(rootEntity);
                    entities.AddRange(EntityManager.ExtractChildEntitiesFromDto(dto));
                    models.Add(EntityManager.ConstructModelFromRootEntity(rootEntity, connection));
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