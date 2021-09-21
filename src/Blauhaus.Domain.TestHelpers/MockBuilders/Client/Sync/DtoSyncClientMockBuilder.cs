using System;
using System.Collections.Generic;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Responses;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync
{
    public class DtoSyncClientMockBuilder : DtoSyncClientMockBuilder<DtoSyncClientMockBuilder, IDtoSyncHandler>
    {
    }

    public abstract class DtoSyncClientMockBuilder<TBuilder, TMock> : BaseAsyncPublisherMockBuilder<TBuilder, TMock, DtoSyncStatus>
        where TBuilder: DtoSyncClientMockBuilder<TBuilder, TMock>
        where TMock : class, IDtoSyncHandler
    {

        public TBuilder Where_SyncDtoAsync_returns(Response value)
        {
            Mock.Setup(x => x.SyncDtoAsync(It.IsAny<IKeyValueProvider?>()))
                .ReturnsAsync(value);
            return (TBuilder)this;
        }  


    }
}