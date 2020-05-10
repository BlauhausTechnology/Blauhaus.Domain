using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Common.Time.Service;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests
{
    public class LoadMoreTests : BaseDomainTest<SyncClient<TestModel, TestModelDto, TestSyncCommand>>
    {
        private ClientSyncRequirement _clientSyncRequirement;
        private TestSyncCommand _syncCommand;
        private List<string> _statusMessages;
        private List<SyncClientState> _stateUpdates;
        private MockBuilder<ISyncStatusHandler> MockSyncStatusHandler => AddMock<ISyncStatusHandler>().Invoke();
        private ConnectivityServiceMockBuilder MockConnectivityService => AddMock<ConnectivityServiceMockBuilder, IConnectivityService>().Invoke();
        private MockBuilder<ITimeService> MockTimeService => AddMock<ITimeService>().Invoke();

        private SyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand> MockSyncClientRepository
            => AddMock<SyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand>, ISyncClientRepository<TestModel, TestModelDto, TestSyncCommand>>().Invoke();

        private CommandHandlerMockBuilder<SyncResult<TestModel>, TestSyncCommand> MockSyncCommandHandler
            => AddMock<CommandHandlerMockBuilder<SyncResult<TestModel>, TestSyncCommand>, ICommandHandler<SyncResult<TestModel>, TestSyncCommand>>().Invoke();

        public override void Setup()
        {
            base.Setup();
            _syncCommand = new TestSyncCommand
            {
                BatchSize = 3,
                FavouriteFood = "Lasagne"
            };
            _clientSyncRequirement = ClientSyncRequirement.Batch;

            MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel> {EntityBatch = new List<TestModel>()});
            MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus());
            MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(new List<TestModel>());
            MockSyncStatusHandler.Mock.SetupAllProperties();
            
            _statusMessages = new List<string>();
            MockSyncStatusHandler.Mock.SetupSet(x => x.StatusMessage)
                .Callback(message => _statusMessages.Add(message));

            _stateUpdates = new List<SyncClientState>();
            MockSyncStatusHandler.Mock.SetupSet(x => x.State)
                .Callback(state => _stateUpdates.Add(state));

            AddService(MockSyncClientRepository.Object);
            AddService(MockSyncCommandHandler.Object);
            AddService(MockTimeService.Object);
            AddService(MockConnectivityService.Object);
        }

        [Test]
        public async Task WHEN_another_batch_exists_locally_SHOULD_load_and_publish_it()
        {
            //Arrange
            var publishedModels = new List<TestModel>();
            var localModels = TestModel.GenerateList(6).OrderByDescending(x => x.ModifiedAtTicks).ToList();
            var firstBatchOfLocalModels = localModels.Take(3).ToList(); 
            MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(firstBatchOfLocalModels);
            MockSyncClientRepository.Where_GetSyncStatusAsync_returns( 
                new ClientSyncStatus
                {
                    AllLocalEntities = 6,
                    SyncedLocalEntities = 6,
                    NewestModifiedAt = firstBatchOfLocalModels.First().ModifiedAtTicks,
                    OldestModifiedAt = firstBatchOfLocalModels.Last().ModifiedAtTicks
                });
            Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                .Subscribe(next => publishedModels.Add(next));
            publishedModels.Clear();

            var secondBatchOfLocalModels = localModels.Skip(3).Take(3).ToList(); 
            MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(secondBatchOfLocalModels);
            MockSyncClientRepository.Mock.Invocations.Clear(); 

            //Act
            Sut.LoadMore();
            await Task.Delay(20);

            //Assert
            MockSyncClientRepository.Mock.Verify(x => x.LoadModelsAsync(It.Is<TestSyncCommand>(y => 
                y.FavouriteFood == "Lasagne" &&
                y.BatchSize == 3 && 
                y.NewerThan == null && 
                y.OlderThan == firstBatchOfLocalModels.Last().ModifiedAtTicks)));
            Assert.AreEqual(3, publishedModels.Count);
            Assert.AreEqual(secondBatchOfLocalModels[0].Id, publishedModels[0].Id);
            Assert.AreEqual(secondBatchOfLocalModels[1].Id, publishedModels[1].Id);
            Assert.AreEqual(secondBatchOfLocalModels[2].Id, publishedModels[2].Id);

            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 5);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 6);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 7, Times.Never);

            Assert.AreEqual(10, _stateUpdates.Count);
            Assert.AreEqual(SyncClientState.Starting, _stateUpdates[0]);
            Assert.AreEqual(SyncClientState.Starting, _stateUpdates[1]);
            Assert.AreEqual(SyncClientState.LoadingLocal, _stateUpdates[2]);
            Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[3]);
            Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[4]);
            Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[5]);
            Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[6]);
            Assert.AreEqual(SyncClientState.Completed, _stateUpdates[7]);
            Assert.AreEqual(SyncClientState.LoadingLocal, _stateUpdates[8]);
            Assert.AreEqual(SyncClientState.Completed, _stateUpdates[9]);
            
            Assert.AreEqual(8, _statusMessages.Count);
            Assert.AreEqual("TestModel SyncClient connected. Required: All (batch size 3)", _statusMessages[0]);
            Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 6. (total: 6)", _statusMessages[1]);
            Assert.AreEqual("Loading data from local store", _statusMessages[2]);
            Assert.AreEqual("Loaded 3 local models", _statusMessages[3]);
            Assert.AreEqual("0 newer TestModel entities downloaded (0 in total). 0 of 0 still to download", _statusMessages[4]);
            Assert.AreEqual("0 older TestModel entities downloaded (0 in total). 0 of 0 still to download", _statusMessages[5]);
            Assert.AreEqual("LoadMore invoked. Loading next 3 of 6 from device", _statusMessages[6]);
            Assert.AreEqual("LoadMore completed.", _statusMessages[7]);
        }

                
        [Test]
        public async Task WHEN_another_batch_does_not_exist_locally_SHOULD_load_and_publish_fromserver()
        {
            //Arrange
            var publishedModels = new List<TestModel>();
            var models = TestModel.GenerateList(6).OrderByDescending(x => x.ModifiedAtTicks).ToList();
            var localModels = models.Take(3).ToList(); 
            MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(localModels);
            MockSyncClientRepository.Where_GetSyncStatusAsync_returns( 
                new ClientSyncStatus
                {
                    AllLocalEntities = 3,
                    SyncedLocalEntities = 3,
                    NewestModifiedAt = localModels.First().ModifiedAtTicks,
                    OldestModifiedAt = localModels.Last().ModifiedAtTicks
                });
            Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                .Subscribe(next => publishedModels.Add(next));
            publishedModels.Clear();

            var serverModels = models.Skip(3).Take(3).ToList(); 
            MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>
            {
                EntityBatch = serverModels
            });
            MockSyncCommandHandler.Mock.Invocations.Clear(); 

            //Act
            Sut.LoadMore();
            await Task.Delay(20);

            //Assert
            MockSyncCommandHandler.Mock.Verify(x => x.HandleAsync(It.Is<TestSyncCommand>(y => 
                y.FavouriteFood == "Lasagne" &&
                y.BatchSize == 3 && 
                y.NewerThan == null && 
                y.OlderThan == localModels.Last().ModifiedAtTicks), It.IsAny<CancellationToken>()));
            Assert.AreEqual(3, publishedModels.Count);
            Assert.AreEqual(serverModels[0].Id, publishedModels[0].Id);
            Assert.AreEqual(serverModels[1].Id, publishedModels[1].Id);
            Assert.AreEqual(serverModels[2].Id, publishedModels[2].Id);

            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 5);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 6);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 7, Times.Never);

            Assert.AreEqual(10, _stateUpdates.Count);
            Assert.AreEqual(SyncClientState.Starting, _stateUpdates[0]);
            Assert.AreEqual(SyncClientState.Starting, _stateUpdates[1]);
            Assert.AreEqual(SyncClientState.LoadingLocal, _stateUpdates[2]);
            Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[3]);
            Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[4]);
            Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[5]);
            Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[6]);
            Assert.AreEqual(SyncClientState.Completed, _stateUpdates[7]);
            Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[8]);
            Assert.AreEqual(SyncClientState.Completed, _stateUpdates[9]);
            
            Assert.AreEqual(8, _statusMessages.Count);
            Assert.AreEqual("TestModel SyncClient connected. Required: All (batch size 3)", _statusMessages[0]);
            Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 3. (total: 3)", _statusMessages[1]);
            Assert.AreEqual("Loading data from local store", _statusMessages[2]);
            Assert.AreEqual("Loaded 3 local models", _statusMessages[3]);
            Assert.AreEqual("0 newer TestModel entities downloaded (0 in total). 0 of 0 still to download", _statusMessages[4]);
            Assert.AreEqual("0 older TestModel entities downloaded (0 in total). 0 of 0 still to download", _statusMessages[5]);
            Assert.AreEqual("LoadMore invoked. Loading next 3 of 6 from server", _statusMessages[6]);
            Assert.AreEqual("LoadMore completed.", _statusMessages[7]);
        }


    }
}