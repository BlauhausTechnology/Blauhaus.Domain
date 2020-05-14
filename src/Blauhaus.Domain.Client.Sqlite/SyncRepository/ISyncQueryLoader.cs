using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public interface ISyncQueryLoader<TSyncCommand> 
        where TSyncCommand : SyncCommand
    {
        Query GenerateQuery(TSyncCommand syncCommand);
    }
}