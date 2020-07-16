using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Model;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Common.Errors;
using Blauhaus.Domain.Common.Extensions;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.Errors;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncModelTests
{
    public class ConnectTests : BaseDomainTest<SyncModel<TestModel, TestSyncCommand>>
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
            Sut.Connect(_id).Subscribe();

            //Assert
            MockSyncClient.Mock.Verify(x => x.Connect(It.Is<TestSyncCommand>(y => 
                y.Id == _id &&
                y.FavouriteColour == default &&
                y.FavouriteFood == default &&
                y.BatchSize == 1 &&
                y.NewerThan == null &&
                y.NewerThan == null), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()));
        }

        [Test]
        public async Task SHOULD_publish_returned_models_if_ids_match()
        {
            //Arrange
            MockSyncClient.Where_Connect_returns(new List<TestModel>
            {
                new TestModel(_id, EntityState.Active, 1000, "1"),
                new TestModel(Guid.NewGuid(), EntityState.Active, 1000, "2"),
                new TestModel(_id, EntityState.Active, 1000, "3"),
            });
            var tcs = new TaskCompletionSource<List<TestModel>>();

            //Act
            var models = new List<TestModel>();
            Sut.Connect(_id).Subscribe(next =>
            {
                models.Add(next);
                if (models.Count == 2)
                {
                    tcs.SetResult(models);
                }
            });
            await tcs.Task;

            //Assert
            Assert.That(models[0].Id, Is.EqualTo(_id));
            Assert.That(models[1].Id, Is.EqualTo(_id));
        }

        [Test]
        public async Task IF_no_models_exist_on_server_SHOULD_fail()
        {
            //Arrange
            var tcs = new TaskCompletionSource<Exception>();

            //Act
            Sut.Connect(_id).Subscribe(next => { }, error =>
            {
                tcs.SetResult(error);
            });
            MockSyncStatusHandler.With(x => x.AllServerEntities, 0);
            MockSyncStatusHandler.RaisePropertyChanged();
            var result = await tcs.Task;

            //Assert
            Assert.That(result, Is.InstanceOf<ErrorException>());
            Assert.That(((ErrorException)result).Error, Is.EqualTo(DomainErrors.NotFound()));
            MockAnalyticsService.VerifyTrace(DomainErrors.NotFound<TestModel>().ToString(), LogSeverity.Warning);
        }

        [Test]
        public async Task IF_sync_fails_SHOULD_fail()
        {
            //Arrange
            var tcs = new TaskCompletionSource<Exception>();
            MockSyncClient.Where_Connect_returns_exception(new Exception("oops"));

            //Act
            Sut.Connect(_id).Subscribe(next => { }, error =>
            {
                tcs.SetResult(error);
            });
            var result = await tcs.Task;

            //Assert
            Assert.That(result.Message, Is.EqualTo("oops"));
        }
    }
}