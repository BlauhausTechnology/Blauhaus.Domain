using System;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.ModelLoader;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.ModelLoaderTests
{
    public class LoadTests : BaseDomainTest<ModelLoader<TestModel, TestSyncCommand>>
    {

        private Guid _id;
        
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();

        
        public override void Setup()
        {
            base.Setup();

            _id = Guid.NewGuid();

            AddService(x => MockSyncClient.Object);

        }


        [Test]
        public void SHOULD_connect_using_given_Id()
        {
            //Act
            //Sut.Load(_id).Subscribe();

            //Assert
            //MockSyncClient.Mock.Verify(x => x.Connect(It.Is<TestSyncCommand>(y => 
            //    y.Id == _id), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()));
        }
    }
}