using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Sync.Abstractions;
using SqlKata;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public interface ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>        //TRootEntity is needed to separate one SyncCommand from another for IoC purposes
        where TSyncCommand : SyncCommand
        where TRootEntity : ISyncClientEntity
    {
        Query GenerateQuery(TSyncCommand syncCommand);
    }
}