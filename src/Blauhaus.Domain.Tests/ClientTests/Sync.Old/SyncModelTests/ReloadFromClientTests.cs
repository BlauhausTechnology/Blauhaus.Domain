﻿using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.Sync.Old.Model;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.Sync.Old.SyncModelTests
{
    public class ReloadFromClientTests : BaseDomainTest<SyncModel<TestModel, TestSyncCommand>>
    {
        private Guid _id;

        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();

        public override void Setup()
        {
            base.Setup();
            
            MockSyncClient.Where_Connect_returns(new List<TestModel>());

            _id = Guid.NewGuid();
            AddService(MockSyncClient.Object);

        }

        [Test]
        public void SHOULD_connect_to_sync_client()
        {
            //Act
            Sut.LoadNewFromClient();

            //Assert
            MockSyncClient.Mock.Verify(x => x.LoadNewFromClient());
        }
    }
}