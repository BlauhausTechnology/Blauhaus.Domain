using System.Collections.Generic;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncManagerTests.Base
{
    public abstract class BaseSyncManagerTest : BaseDomainTest<SyncManager>
    {

        protected DtoSyncClientMockBuilder MockSyncClient1 = null!;
        protected DtoSyncClientMockBuilder MockSyncClient2 = null!;
        protected DtoSyncClientMockBuilder MockSyncClient3 = null!;
        
        protected IKeyValueProvider MockKeyValueProvider = new MockBuilder<IKeyValueProvider>().Object;

        public override void Setup()
        {
            base.Setup();

            MockSyncClient1 = new DtoSyncClientMockBuilder();
            MockSyncClient2 = new DtoSyncClientMockBuilder();
            MockSyncClient3 = new DtoSyncClientMockBuilder();

            AddService(MockSyncClient1.Object);
            AddService(MockSyncClient2.Object);
            AddService(MockSyncClient3.Object);
        }
    }
}