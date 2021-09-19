using System;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.SyncClient;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.DtoSyncClientTests.Base
{
    public class BaseDtoSyncClientTest : BaseDomainTest<DtoSyncClient<MyDto, Guid>>
    {
        protected CommandHandlerMockBuilder<IDtoBatch<MyDto>, DtoSyncCommand> MockSyncCommandHandler = null!;
        protected SyncDtoCacheMockBuilder<MyDto, Guid> MockSyncDtoCache = null!;
        protected MockBuilder<IDtoSyncConfig> MockSyncDtoConfig = null!;

        public override void Setup()
        {
            base.Setup();

            MockSyncCommandHandler = new CommandHandlerMockBuilder<IDtoBatch<MyDto>, DtoSyncCommand>();
            AddService(MockSyncCommandHandler.Object);

            MockSyncDtoCache = new SyncDtoCacheMockBuilder<MyDto, Guid>();
            AddService(MockSyncDtoCache.Object);

            MockSyncDtoConfig = new MockBuilder<IDtoSyncConfig>();
            MockSyncDtoConfig.Setup(x => x.GetSyncBatchSize("MyDto"), 10);
            AddService(MockSyncDtoConfig.Object);
        }
    }
}