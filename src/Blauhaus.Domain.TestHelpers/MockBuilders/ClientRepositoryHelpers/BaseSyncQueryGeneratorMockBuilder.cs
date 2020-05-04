using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SqlKata;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.ClientRepositoryHelpers
{

    public class SyncQueryGeneratorMockBuilder<TMock, TSyncCommand> : BaseSyncQueryGeneratorMockBuilder<SyncQueryGeneratorMockBuilder<TMock, TSyncCommand>, ISyncQueryGenerator<TSyncCommand>, TSyncCommand>
    {

    }


    public abstract class BaseSyncQueryGeneratorMockBuilder<TBuilder, TMock, TSyncCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ISyncQueryGenerator<TSyncCommand> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
    {
        protected BaseSyncQueryGeneratorMockBuilder()
        {
            Mock.Setup(x => x.ExtendQuery(It.IsAny<Query>(), It.IsAny<TSyncCommand>()))
                .Returns((Query q, TSyncCommand c) => q);
        }

        public TBuilder Where_ExtendQuery_returns(Query query)
        {
            Mock.Setup(x => x.ExtendQuery(It.IsAny<Query>(), It.IsAny<TSyncCommand>()))
                .Returns(query);
            return this as TBuilder;
        }
    }
}