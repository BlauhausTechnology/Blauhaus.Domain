using System;
using System.Collections.Generic;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Client.Sync.SyncClient;
using Blauhaus.Responses;
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

        public TBuilder Where_SyncDtoAsync_returns(Response value)
        {
            Mock.Setup(x => x.SyncDtoAsync(It.IsAny<IKeyValueProvider?>()))
                .ReturnsAsync(value);
            return (TBuilder)this;
        }  


    }
}