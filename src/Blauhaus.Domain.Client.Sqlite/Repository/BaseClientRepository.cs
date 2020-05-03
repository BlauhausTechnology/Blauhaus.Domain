using System;
using System.Threading.Tasks;
using Blauhaus.ClientDatabase.Sqlite.Entities;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public abstract class BaseSqliteRepository<TModel, TRootEntity, TDto> : IClientRepository<TModel, TDto> 
        where TModel : class, IClientEntity
        where TRootEntity : BaseSqliteEntity, new()
    {
        protected readonly ISqliteDatabaseService DatabaseService;
        protected readonly IClientRepositoryHelper<TModel, TRootEntity, TDto> Helper;

        protected BaseSqliteRepository(
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientRepositoryHelper<TModel, TRootEntity, TDto> helper)
        {
            DatabaseService = sqliteDatabaseService;
            Helper = helper;
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
                    model = Helper.ConstructModelFromRootEntity(entity, connection);
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
                var rootEntity = Helper.ExtractRootEntityFromDto(dto);
                connection.InsertOrReplace(rootEntity);

                foreach (var childEntity in Helper.ExtractChildEntitiesFromDto(dto))
                {
                    connection.InsertOrReplace(childEntity);
                }
                connection.InsertOrReplace(rootEntity);

                model = Helper.ConstructModelFromRootEntity(rootEntity, connection);
            });

            return model;

        }
    }
}