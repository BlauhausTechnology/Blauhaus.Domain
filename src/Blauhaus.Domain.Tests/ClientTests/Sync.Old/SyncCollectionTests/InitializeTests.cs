using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Sync.Old.Collection;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.ListItems;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.Sync.Old.SyncCollectionTests
{
    [TestFixture]
    public class InitializeTests : BaseDomainTest<SyncCollection<TestModel, ITestListItem, TestSyncCommand>>
    {
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();

        private DateTime _start;

        public override void Setup()
        {
            base.Setup();

            _start = DateTime.UtcNow;
            
            MockSyncClient.Where_Connect_returns(new List<TestModel>());

            AddService(MockSyncClient.Object);

            MockServiceLocator.Where_Resolve_returns_sequence(new List<ITestListItem>
            {
                new TestBaseListItem(),new TestBaseListItem(),new TestBaseListItem()
            });
        }
         

        [Test]
        public void SHOULD_connect_using_configured_properties()
        {
            //Act
            Services.AddTransient<IListItem<TestModel>, TestBaseListItem>();
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
        public void IF_elementUpdater_throws_exception_SHOULD_handle()
        {
            //Arrange
            var newModels = TestModel.GenerateList(3, _start);
            MockSyncClient.Where_Connect_returns(newModels);
            MockServiceLocator.Where_Resolve_returns(new ListItemMockBuilder<ITestListItem, TestModel>()
                .Where_Update_throws(new Exception("This is an exceptionally bad thing that just happened")).Object);

            //Act
            Sut.Initialize();

            //Assert 
            MockErrorHandler.Mock.Verify(x => x.HandleExceptionAsync(Sut, It.Is<Exception>(y => y.Message == "This is an exceptionally bad thing that just happened")));
        }

        [Test]
        public void SHOULD_publish_new_models_as_listitems()
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
        public void IF_ListItem_Update_method_returns_false_SHOULDnot_add()
        {
            //Arrange
            MockSyncClient.Where_Connect_returns(TestModel.GenerateList(1, _start));
            MockServiceLocator.Where_Resolve_returns(new ListItemMockBuilder<ITestListItem, TestModel>()
                .Where_Update_returns(false).Object);

            //Act
            Sut.Initialize();

            //Assert
            Assert.AreEqual(0, Sut.ListItems.Count);
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

        [Test]
        public async Task WHEN_updating_listitem_returns_false_SHOULD_remove()
        {
            //Arrange
            var update1 = new TestModel(Guid.NewGuid(), EntityState.Active, 1000, "A");
            var update2 = new TestModel(update1.Id, EntityState.Active, 2000, "B"); 
            MockSyncClient.Where_Connect_returns(new List<TestModel>{update1, update2 });
            MockServiceLocator.Where_Resolve_returns<ITestListItem>(new ListItemMockBuilder<ITestListItem, TestModel>()
                .With(x => x.Id, update1.Id)
                .Where_Update_returns_sequence(true, false).Object);

            //Act
            Sut.Initialize();
            await Task.Delay(100);

            //Assert
            Assert.AreEqual(0, Sut.ListItems.Count); 
        }


    }
}