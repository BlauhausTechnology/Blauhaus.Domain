using System.Collections.Generic;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Sync.Old.Collection;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync.Old.SyncClients;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.Sync.Old.SyncCollectionTests
{
    [TestFixture]
    public class LoadNewFromClientTests : BaseDomainTest<SyncCollection<TestModel, TestBaseListItem, TestSyncCommand>>
    {
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();


        public override void Setup()
        {
            base.Setup();

            MockSyncClient.Where_Connect_returns(new List<TestModel>());

            AddService(MockSyncClient.Object);
        }
         

        [Test]
        public void SHOULD_ReloadFromClient()
        {
            //Arrange
            Sut.SyncCommand.FavouriteColour = "Red";
            Sut.SyncRequirement = ClientSyncRequirement.Minimum(100);
            Sut.Initialize();

            //Act
            Sut.LoadNewFromClient();

            //Assert
            MockSyncClient.Mock.Verify(x => x.LoadNewFromClient());
        }
         
    }
}