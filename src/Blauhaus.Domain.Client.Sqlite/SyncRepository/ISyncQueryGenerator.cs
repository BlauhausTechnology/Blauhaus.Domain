using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public interface ISyncQueryGenerator<TSyncCommand>
    {
        Query ExtendQuery(Query query, TSyncCommand syncCommand);
    }
}