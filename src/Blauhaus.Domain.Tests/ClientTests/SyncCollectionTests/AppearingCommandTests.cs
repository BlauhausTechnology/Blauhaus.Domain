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
    public class AppearingCommandTests : BaseDomainTest<SyncCollection<TestModel, TestListItem, TestSyncCommand>>
    {
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();

        private DateTime _start;

        public override void Setup()
        {
            base.Setup();

            _start = DateTime.UtcNow;
            
            MockSyncClient.Where_Connect_returns(new List<TestModel>());

            Services.AddSingleton<IListItemUpdater<TestModel, TestListItem>, TestViewElementUpdater>();
            AddService(MockSyncClient.Object);
        }
         

        [Test]
        public void SHOULD_connect_using_configured_properties()
        {
            //Act
            Sut.SyncCommand.FavouriteColour = "Red";
            Sut.SyncRequirement = ClientSyncRequirement.Minimum(100);
            Sut.Initialize();

            //Assert
            MockSyncClient.Mock.Verify(x => x.Connect(
                It.Is<TestSyncCommand>(y => y.FavouriteColour == "Red"), 
                It.Is<ClientSyncRequirement>(z => z.SyncMinimumQuantity.Value == 100), 
                Sut.SyncStatusHandler));
        }
        

        [Test]
        public void IF_already_initialized_SHOULD_LoadNewFromClient()
        {
            //Act
            Sut.SyncCommand.FavouriteColour = "Red";
            Sut.SyncRequirement = ClientSyncRequirement.Minimum(100);
            Sut.Initialize();
            Sut.Initialize();

            //Assert
            MockSyncClient.Mock.Verify(x => x.Connect(
                It.Is<TestSyncCommand>(y => y.FavouriteColour == "Red"), 
                It.Is<ClientSyncRequirement>(z => z.SyncMinimumQuantity.Value == 100), 
                Sut.SyncStatusHandler), Times.Once);
            MockSyncClient.Mock.Verify(x => x.LoadNewFromClient());
        }
        
        [Test]
        public void IF_connect_returns_exception_SHOULD_handle()
        {
            //Arrange
            MockSyncClient.Where_Connect_returns_exception(new Exception("oops"));

            //Act
            Sut.Initialize();

            //Assert
            MockErrorHandler.Mock.Verify(x => x.HandleExceptionAsync(Sut, It.Is<Exception>(y => y.Message == "oops")));
        }
        
        [Test]
        public void IF_elementUpdater_thorws_exception_SHOULD_handle()
        {
            //Arrange
            var newModels = TestModel.GenerateList(3, _start);
            MockSyncClient.Where_Connect_returns(newModels);
            Services.AddSingleton<IListItemUpdater<TestModel, TestListItem>, ExceptionViewElementUpdater>();

            //Act
            Sut.Initialize();

            //Assert 
            MockErrorHandler.Mock.Verify(x => x.HandleExceptionAsync(Sut, It.Is<Exception>(y => y.Message == "This is an exceptionally bad thing that just happened")));
        }

        [Test]
        public void SHOULD_publish_new_models()
        {
            //Arrange
            var newModels = TestModel.GenerateList(3, _start);
            MockSyncClient.Where_Connect_returns(newModels);

            //Act
            Sut.Initialize();

            //Assert
            Assert.AreEqual(3, Sut.ListItems.Count);
            Assert.AreEqual(newModels[0].Id, Sut.ListItems[0].Id);
            Assert.AreEqual(newModels[0].Name, Sut.ListItems[0].Name);
            Assert.AreEqual(newModels[1].Id, Sut.ListItems[1].Id);
            Assert.AreEqual(newModels[1].Name, Sut.ListItems[1].Name);
            Assert.AreEqual(newModels[2].Id, Sut.ListItems[2].Id);
            Assert.AreEqual(newModels[2].Name, Sut.ListItems[2].Name);
        }
        
        [Test]
        public void SHOULD_publish_models_most_recent_first()
        {
            //Arrange
            MockSyncClient.Where_Connect_returns(new List<TestModel>
            {
                new TestModel(Guid.NewGuid(), EntityState.Active, 2000, "A"),
                new TestModel(Guid.NewGuid(), EntityState.Active, 1000, "B"),
                new TestModel(Guid.NewGuid(),EntityState.Active, 3000, "C"),
            });

            //Act
            Sut.Initialize();

            //Assert
            Assert.AreEqual(3000, Sut.ListItems[0].ModifiedAtTicks);
            Assert.AreEqual(2000, Sut.ListItems[1].ModifiedAtTicks);
            Assert.AreEqual(1000, Sut.ListItems[2].ModifiedAtTicks);
        }

        [Test]
        public void WHEN_model_Changes_SHOULD_update_it_and_reorder_instead_of_adding_new()
        {
            //Arrange
            var update1 = new TestModel(Guid.NewGuid(), EntityState.Active, 1000, "A");
            var update2 = new TestModel(Guid.NewGuid(), EntityState.Active, 2000, "B");
            var update3 = new TestModel(update1.Id, EntityState.Active, 4000, "D");
            MockSyncClient.Where_Connect_returns(new List<TestModel>{update1, update2, update3});

            //Act
            Sut.Initialize();

            //Assert
            Assert.AreEqual(2, Sut.ListItems.Count);
            Assert.AreEqual(4000, Sut.ListItems[0].ModifiedAtTicks);
            Assert.AreEqual("D", Sut.ListItems[0].Name);
            Assert.AreEqual(update1.Id, Sut.ListItems[0].Id);
            Assert.AreEqual(2000, Sut.ListItems[1].ModifiedAtTicks);
            Assert.AreEqual("B", Sut.ListItems[1].Name);
            Assert.AreEqual(update2.Id, Sut.ListItems[1].Id);

        }


    }
}