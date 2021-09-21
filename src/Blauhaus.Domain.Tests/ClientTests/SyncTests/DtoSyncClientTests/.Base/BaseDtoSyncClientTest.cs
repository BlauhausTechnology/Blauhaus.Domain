using System;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.DtoSyncClientTests.Base
{
    public class BaseDtoSyncClientTest : BaseDomainTest<DtoSyncHandler<MyDto, Guid>>
    {
        protected CommandHandlerMockBuilder<IDtoBatch<MyDto>, DtoSyncCommand> MockSyncCommandHandler = null!;
        protected SyncDtoCacheMockBuilder<MyDto, Guid> MockSyncDtoCache = null!;
        protected IKeyValueProvider MockKeyValueProvider = new MockBuilder<IKeyValueProvider>().Object;

        public override void Setup()
        {
            base.Setup();

            MockSyncCommandHandler = new CommandHandlerMockBuilder<IDtoBatch<MyDto>, DtoSyncCommand>();
            AddService(MockSyncCommandHandler.Object);

            MockSyncDtoCache = new SyncDtoCacheMockBuilder<MyDto, Guid>();
            AddService(MockSyncDtoCache.Object);
        }
    }
}