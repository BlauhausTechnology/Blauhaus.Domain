using System;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SqlKata;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientRepositoryHelpers
{

    public class SyncQueryGeneratorMockBuilder<TMock, TRootEntity, TSyncCommand> 
        : BaseSyncQueryGeneratorMockBuilder<SyncQueryGeneratorMockBuilder<TMock, TRootEntity, TSyncCommand>, ISyncQueryGenerator<TRootEntity, TSyncCommand>,TRootEntity,  TSyncCommand>
        where TRootEntity : ISyncClientEntity 
        where TSyncCommand : SyncCommand
    {

    }


    public abstract class BaseSyncQueryGeneratorMockBuilder<TBuilder, TMock, TRootEntity, TSyncCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ISyncQueryGenerator<TRootEntity, TSyncCommand> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
        where TRootEntity : ISyncClientEntity
        where TSyncCommand : SyncCommand
    {
 

        public TBuilder Where_ExtendQuery_returns(Func<Query> query) 
        {
            Mock.Setup(x => x.GenerateQuery(It.IsAny<TSyncCommand>()))
                .Returns(query.Invoke);
            return this as TBuilder;
        }

    }
}