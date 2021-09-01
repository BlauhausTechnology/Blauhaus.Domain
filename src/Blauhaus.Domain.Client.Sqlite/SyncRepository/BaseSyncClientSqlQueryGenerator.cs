using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using SqlKata;
using System;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public abstract class BaseSyncClientSqlQueryGenerator<TSyncCommand, TRootEntity> : ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>
        where TRootEntity : ISyncClientEntity<Guid>
        where TSyncCommand : SyncCommand
    {
        public Query GenerateQuery(TSyncCommand syncCommand)
        {
            var query = new Query(typeof(TRootEntity).Name);
            return ConfigureQuery(syncCommand, query);
        }

        protected abstract Query ConfigureQuery(TSyncCommand syncCommand, Query query);
    }
}