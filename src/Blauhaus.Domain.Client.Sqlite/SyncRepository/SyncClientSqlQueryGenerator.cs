using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using SqlKata;
using System;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class SyncClientSqlQueryGenerator<TRootEntity> : BaseSyncClientSqlQueryGenerator<SyncCommand, TRootEntity>
        where TRootEntity : ISyncClientEntity<Guid>
    {
        
        protected override Query ConfigureQuery(SyncCommand syncCommand, Query query)
        {
            return query;
        }
    }
}