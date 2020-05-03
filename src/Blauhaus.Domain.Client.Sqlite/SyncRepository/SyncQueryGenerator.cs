using Blauhaus.Domain.Common.CommandHandlers.Sync;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public class SyncQueryGenerator : ISyncQueryGenerator<SyncCommand>
    {
        public Query ExtendQuery(Query query, SyncCommand syncCommand)
        {
            return query;
        }
    }
}