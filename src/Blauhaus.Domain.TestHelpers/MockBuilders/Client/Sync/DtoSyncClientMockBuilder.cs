using System;
using System.Collections.Generic;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Client.Sync.SyncClient;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync
{
    public class DtoSyncClientMockBuilder : DtoSyncClientMockBuilder<DtoSyncClientMockBuilder, IDtoSyncClient>
    {
    }

    public abstract class DtoSyncClientMockBuilder<TBuilder, TMock> : BaseAsyncPublisherMockBuilder<TBuilder, TMock, DtoSyncStatus>
        where TBuilder: DtoSyncClientMockBuilder<TBuilder, TMock>
        where TMock : class, IDtoSyncClient
    {

        public TBuilder Where_GetLastModifiedTicksAsync_returns(string dtoName, long value)
        {
            Mock.Setup(x => x.LoadLastModifiedTicksAsync())
                .ReturnsAsync(new KeyValuePair<string, long>(dtoName, value));
            return (TBuilder)this;
        }  


    }
}