using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class DefaultSyncQueryGenerator<TRootEntity> : ISyncQueryLoader<TRootEntity, SyncCommand>
        where TRootEntity : ISyncClientEntity
    {
        public Query GenerateQuery(SyncCommand syncCommand)
        {
            return new Query(typeof(TRootEntity).Name);
        }
    }
}