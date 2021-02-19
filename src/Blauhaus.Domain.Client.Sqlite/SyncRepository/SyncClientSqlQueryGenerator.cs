using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class SyncClientSqlQueryGenerator<TRootEntity> : BaseSyncClientSqlQueryGenerator<SyncCommand, TRootEntity>
        where TRootEntity : ISyncClientEntity
    {
        
        protected override Query ConfigureQuery(SyncCommand syncCommand, Query query)
        {
            return query;
        }
    }
}