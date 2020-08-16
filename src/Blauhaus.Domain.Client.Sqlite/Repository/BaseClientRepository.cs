using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Abstractions.Entities;
using SqlKata;
using SqlKata.Compilers;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public abstract class BaseClientRepository<TModel, TDto, TRootEntity> : IClientRepository<TModel, TDto> 
        where TModel : class, IClientEntity
        where TRootEntity : ISyncClientEntity, new()
    {
        protected readonly IAnalyticsService AnalyticsService;
        protected readonly ISqliteDatabaseService DatabaseService;
        protected readonly IClientEntityConverter<TModel, TDto, TRootEntity> EntityConverter;

        protected readonly SqliteCompiler SqlCompiler = new SqliteCompiler();
        protected virtual Query CreateSqlQuery() => new Query(typeof(TRootEntity).Name);

        protected BaseClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityConverter<TModel, TDto, TRootEntity> entityConverter)
        {
            AnalyticsService = analyticsService;
            DatabaseService = sqliteDatabaseService;
            EntityConverter = entityConverter;
        }


        public async Task<TModel?> LoadByIdAsync(Guid id)
        {
            var models = await LoadAsync(x => x.Id == id);
            return models.FirstOrDefault(); 
        }

        protected async Task<IReadOnlyList<TModel>> LoadAsync(Expression<Func<TRootEntity, bool>> predicate)
        {
            var db = await DatabaseService.GetDatabaseConnectionAsync();
            var models = new List<TModel>();

            await db.RunInTransactionAsync(connection =>
            {
                var rootEntities = connection.Table<TRootEntity>()
                    .Where(predicate);

                foreach (var rootEntity in rootEntities)
                {
                    var childEntities = EntityConverter.LoadChildEntities(rootEntity, connection);
                    models.Add(EntityConverter.ConstructModel(rootEntity, childEntities));
                }
            });

            return models;
        }

        public async Task<TModel> SaveDtoAsync(TDto dto)
        {
            TModel? model = default;

            var db = await DatabaseService.GetDatabaseConnectionAsync();

            await db.RunInTransactionAsync(connection =>
            {
                var entities = EntityConverter.ExtractEntitiesFromDto(dto);

                var rootEntity = entities.Item1;
                var childEntities = entities.Item2;
                
                foreach (var childEntity in childEntities)
                {
                    connection.InsertOrReplace(childEntity);
                }
                connection.InsertOrReplace(rootEntity);
                
                //in case the download only includes updated child entities and the latest version is already stored locally
                var reloadedChildEntities = EntityConverter.LoadChildEntities(rootEntity, connection);

                model = EntityConverter.ConstructModel(rootEntity, reloadedChildEntities);
            });

            return model;

        }
    }
}