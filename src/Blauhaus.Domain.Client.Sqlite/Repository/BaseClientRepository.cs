﻿using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Common.Entities;
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
            TModel? model = default;

            var db = await DatabaseService.GetDatabaseConnectionAsync();
            await db.RunInTransactionAsync(connection =>
            {
                var rootEntity = connection.Table<TRootEntity>()
                    .FirstOrDefault(x => x.Id == id);

                if (rootEntity != null)
                {
                    var childEntities = EntityConverter.LoadChildEntities(rootEntity, connection);
                    model = EntityConverter.ConstructModel(rootEntity, childEntities);
                }
            });

            return model;
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
                
                model = EntityConverter.ConstructModel(rootEntity, childEntities);
            });

            return model;

        }
    }
}