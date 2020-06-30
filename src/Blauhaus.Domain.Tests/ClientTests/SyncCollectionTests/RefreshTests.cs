using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncCollectionTests
{
    [TestFixture]
    public class RefreshTests : BaseDomainTest<SyncCollection<TestModel, TestListItem, TestSyncCommand>>
    {
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();


        public override void Setup()
        {
            base.Setup();

            MockSyncClient.Where_Connect_returns(new List<TestModel>());

            Services.AddSingleton<IListItemUpdater<TestModel, TestListItem>, TestViewElementUpdater>();
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
            Sut.Refresh();

            //Assert
            MockSyncClient.Mock.Verify(x => x.LoadNewFromServer());
        }
         
    }
}