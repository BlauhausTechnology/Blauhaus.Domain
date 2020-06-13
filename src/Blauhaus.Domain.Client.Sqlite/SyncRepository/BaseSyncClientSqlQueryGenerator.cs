﻿using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public abstract class BaseSyncClientSqlQueryGenerator<TSyncCommand, TRootEntity> : ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>
        where TRootEntity : ISyncClientEntity where TSyncCommand : SyncCommand
    {
        public Query GenerateQuery(TSyncCommand syncCommand)
        {
            var query = new Query(typeof(TRootEntity).Name);
            return ConfigureQuery(syncCommand, query);
        }

        protected abstract Query ConfigureQuery(TSyncCommand syncCommand, Query query);
    }
}