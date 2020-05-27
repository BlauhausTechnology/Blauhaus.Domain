using System;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SqlKata;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientRepositoryHelpers
{

    public class SyncClientSqlQueryGeneratorMockBuilder<TMock, TRootEntity, TSyncCommand> 
        : BaseSyncClientSqlQueryGeneratorMockBuilder<SyncClientSqlQueryGeneratorMockBuilder<TMock, TRootEntity, TSyncCommand>, ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>,TRootEntity,  TSyncCommand>
        where TRootEntity : ISyncClientEntity 
        where TSyncCommand : SyncCommand
    {

    }


    public abstract class BaseSyncClientSqlQueryGeneratorMockBuilder<TBuilder, TMock, TRootEntity, TSyncCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
        where TRootEntity : ISyncClientEntity
        where TSyncCommand : SyncCommand
    {
 

        public TBuilder Where_GenerateQuery_returns(Func<Query> query) 
        {
            Mock.Setup(x => x.GenerateQuery(It.IsAny<TSyncCommand>()))
                .Returns(query.Invoke);
            return this as TBuilder;
        }

    }
}