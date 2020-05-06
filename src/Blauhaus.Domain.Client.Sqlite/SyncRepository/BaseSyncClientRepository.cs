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

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class BaseSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity> : BaseClientRepository<TModel,TDto,TRootEntity>, ISyncClientRepository<TModel,TDto, TSyncCommand> 
        where TRootEntity : BaseSyncClientEntity, new() 
        where TModel : class, IClientEntity 
        where TSyncCommand : SyncCommand
    {
        private readonly ISyncQueryGenerator<TSyncCommand> _syncQueryGenerator;

        //TODO unhandled case: when syncing after a long time, and number of entities modified since LastModifiedAtTicks exceeds BatchCount
        //TODO in this case there will be a gap of missing entities modified before the FirstModified of the returned batch and after the LastModified on device
        //TODO these entities will be lost forever
        //TODO the only way I can think to solve this is to track occurences of this case in a sync metadata table and send it to the server so the server can correct
        //TODO this is the pitfall of doing MostRecentFirst syncing, and the solutions are probably ugly. Yup, it's going to be ugly. But cool. 

        //TODO maybe the best way to handle this is to invalidate all client side entities older than the new batch IF we are unable to resolve the problem during a sync cycle

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
                var orderedQuery = connection.Table<TRootEntity>()
                    .Where(x => x.SyncState == SyncState.InSync)
                    .OrderByDescending(x => x.ModifiedAtTicks);
                
                syncStatus.LastModifiedAt = orderedQuery.FirstOrDefault()?.ModifiedAtTicks;
                syncStatus.FirstModifiedAt = orderedQuery.LastOrDefault()?.ModifiedAtTicks ?? 0;
                syncStatus.LocalSyncedEntities = orderedQuery.Count();
                syncStatus.LocalEntities = connection.Table<TRootEntity>().Count();
            });

            return syncStatus;
        }

        public async Task<IReadOnlyList<TModel>> LoadModelsAsync(TSyncCommand syncCommand)
        { 
            var db = await DatabaseService.GetDatabaseConnectionAsync();
            var models = new List<TModel>();

            await db.RunInTransactionAsync(connection =>
            {
                var query = SqlQuery()
                    .OrderByDesc(nameof(IClientEntity.ModifiedAtTicks))
                    .Take(syncCommand.BatchSize);

                if (syncCommand.OlderThan > 0)
                {
                    query = query.Where(nameof(IClientEntity.ModifiedAtTicks), "<", syncCommand.OlderThan);
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
                    rootEntity.SyncState = SyncState.InSync;
                    entities.Add(rootEntity);

                    var childEntities = EntityManager.ExtractChildEntitiesFromDto(dto);
                    foreach (var childEntity in childEntities)
                    {
                        childEntity.SyncState = SyncState.InSync;
                        entities.Add(childEntity);
                    }

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