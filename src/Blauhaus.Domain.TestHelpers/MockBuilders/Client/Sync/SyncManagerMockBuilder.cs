using System.Collections.Generic;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync
{
    public class SyncManagerMockBuilder : BaseAsyncPublisherMockBuilder<SyncManagerMockBuilder, ISyncManager, IOverallSyncStatus>
    {
        public SyncManagerMockBuilder Where_GetLastModifiedTicksAsync_returns(Dictionary<string, long> value)
        {
            Mock.Setup(x => x.GetLastModifiedTicksAsync())
                .ReturnsAsync(value);
            return this;
        }

        public SyncManagerMockBuilder Where_GetLastModifiedTicksAsync_returns(Response value)
        {
            Mock.Setup(x => x.SyncAllAsync(It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(value);
            return this;
        }
    }
}