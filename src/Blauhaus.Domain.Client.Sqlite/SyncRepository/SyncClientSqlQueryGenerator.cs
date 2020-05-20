using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
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