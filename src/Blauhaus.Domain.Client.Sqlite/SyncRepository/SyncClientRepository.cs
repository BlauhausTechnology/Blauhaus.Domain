﻿using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sqlite.Repository;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class SyncClientRepository <TModel, TDto, TSyncCommand, TRootEntity> : BaseSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity>
        where TRootEntity : ISyncClientEntity, new() 
        where TModel : class, IClientEntity 
        where TSyncCommand : SyncCommand
    {
        public SyncClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityConverter<TModel, TDto, TRootEntity> entityConverter,
            ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity> syncQueryLoader) 
                : base(analyticsService, sqliteDatabaseService, entityConverter, syncQueryLoader)
        {
        }
    }
}