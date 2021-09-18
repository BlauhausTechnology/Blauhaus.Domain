using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using SqlKata;
using System;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Client.Sqlite.SyncRepository
{
    public interface ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>        //TRootEntity is needed to separate one SyncCommand from another for IoC purposes
        where TSyncCommand : SyncCommand
        where TRootEntity : ISyncClientEntity<Guid>
    {
        Query GenerateQuery(TSyncCommand syncCommand);
    }
}