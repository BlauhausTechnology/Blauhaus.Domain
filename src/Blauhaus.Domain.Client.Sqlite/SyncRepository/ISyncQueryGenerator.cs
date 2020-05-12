using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public interface ISyncQueryGenerator<TRootEntity, TSyncCommand> 
        where TSyncCommand : SyncCommand
        where TRootEntity : ISyncClientEntity
    {
        Query GenerateQuery(TSyncCommand syncCommand);
    }
}