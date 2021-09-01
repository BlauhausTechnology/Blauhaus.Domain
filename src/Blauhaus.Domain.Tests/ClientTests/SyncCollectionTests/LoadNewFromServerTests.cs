﻿using System.Collections.Generic;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncCollectionTests
{
    [TestFixture]
    public class LoadNewFromServerTests : BaseDomainTest<SyncCollection<TestModel, TestBaseListItem, TestSyncCommand>>
    {
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();


        public override void Setup()
        {
            base.Setup();

            MockSyncClient.Where_Connect_returns(new List<TestModel>());

            AddService(MockSyncClient.Object);
        }
         

        [Test]
        public void SHOULD_ReloadFromServer()
        {
            //Arrange
            Sut.SyncCommand.FavouriteColour = "Red";
            Sut.SyncRequirement = ClientSyncRequirement.Minimum(100);
            Sut.Initialize();

            //Act
            Sut.LoadNewFromServer();

            //Assert
            MockSyncClient.Mock.Verify(x => x.LoadNewFromServer());
        }
         
    }
}