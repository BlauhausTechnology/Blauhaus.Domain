using System.Collections.Generic;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Service;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients
{
    public class SyncStatusHandlerFactoryMockBuilder : BaseMockBuilder<SyncStatusHandlerFactoryMockBuilder, ISyncStatusHandlerFactory>
    {
        public SyncStatusHandlerFactoryMockBuilder Where_Get_returns(List<ISyncStatusHandler> values)
        {
            var queue = new Queue<ISyncStatusHandler>(values);
            Mock.Setup(x => x.Get()).Returns(queue.Dequeue);
            return this;
        }
    }
}