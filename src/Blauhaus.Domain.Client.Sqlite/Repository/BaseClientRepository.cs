using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Operation;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Entities;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.Entities;
using SqlKata;
using SqlKata.Compilers;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public abstract class BaseClientRepository<TModel, TDto, TRootEntity> : IClientRepository<TModel, TDto> 
        where TModel : class, IClientEntity
        where TRootEntity : BaseSqliteEntity, new()
    {
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ISqliteDatabaseService DatabaseService;
        protected readonly IClientEntityManager<TModel, TDto, TRootEntity> EntityManager;

        protected readonly SqliteCompiler SqlCompiler = new SqliteCompiler();
        protected Query SqlQuery() => new Query(typeof(TRootEntity).Name);

        protected BaseClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityManager<TModel, TDto, TRootEntity> entityManager)
        {
            AnalyticsService = analyticsService;
            DatabaseService = sqliteDatabaseService;
            EntityManager = entityManager;
        }


        public async Task<TModel> LoadByIdAsync(Guid id)
        {
            TModel model = default;

            var db = await DatabaseService.GetDatabaseConnectionAsync();
            await db.RunInTransactionAsync(connection =>
            {
                var entity = connection.Table<TRootEntity>()
                    .FirstOrDefault(x => x.Id == id);

                if (entity != null)
                {
                    model = EntityManager.ConstructModelFromRootEntity(entity, connection);
                }

            });

            return model;
        }

        public async Task<TModel> SaveDtoAsync(TDto dto)
        {
            TModel model = default;

            var db = await DatabaseService.GetDatabaseConnectionAsync();

            await db.RunInTransactionAsync(connection =>
            {
                var rootEntity = EntityManager.ExtractRootEntityFromDto(dto);
                connection.InsertOrReplace(rootEntity);

                foreach (var childEntity in EntityManager.ExtractChildEntitiesFromDto(dto))
                {
                    connection.InsertOrReplace(childEntity);
                }
                connection.InsertOrReplace(rootEntity);

                model = EntityManager.ConstructModelFromRootEntity(rootEntity, connection);
            });

            return model;

        }
    }
}