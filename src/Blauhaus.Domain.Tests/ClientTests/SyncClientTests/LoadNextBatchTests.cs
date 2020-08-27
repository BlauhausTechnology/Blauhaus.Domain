using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Tests.ClientTests.SyncClientTests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests
{
    public class LoadNextBatchTests : BaseSyncClientTest
    {
        [Test]
        public async Task WHEN_another_batch_exists_locally_SHOULD_load_and_publish_it()
        {
            //Arrange
            var publishedModels = new List<TestModel>();
            var localModels = TestModel.GenerateList(6).OrderByDescending(x => x.ModifiedAtTicks).ToList();
            var firstBatchOfLocalModels = localModels.Take(3).ToList(); 
            MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(firstBatchOfLocalModels);
            MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns( 
                new ClientSyncStatus
                {
                    AllLocalEntities = 6,
                    SyncedLocalEntities = 6,
                    NewestModifiedAt = firstBatchOfLocalModels.First().ModifiedAtTicks,
                    OldestModifiedAt = firstBatchOfLocalModels.Last().ModifiedAtTicks
                });
            Sut.Connect(SyncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                .Subscribe(next => publishedModels.Add(next));
            publishedModels.Clear();

            var secondBatchOfLocalModels = localModels.Skip(3).Take(3).ToList(); 
            MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(secondBatchOfLocalModels);
            MockBaseSyncClientRepository.Mock.Invocations.Clear(); 

            //Act
            Sut.LoadNextBatch();
            await Task.Delay(20);

            //Assert
            MockBaseSyncClientRepository.Mock.Verify(x => x.LoadModelsAsync(It.Is<TestSyncCommand>(y => 
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

            Assert.AreEqual(10, StateUpdates.Count);
            Assert.AreEqual(SyncClientState.Starting, StateUpdates[0]);
            Assert.AreEqual(SyncClientState.Starting, StateUpdates[1]);
            Assert.AreEqual(SyncClientState.LoadingLocal, StateUpdates[2]);
            Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[3]);
            Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[4]);
            Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[5]);
            Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[6]);
            Assert.AreEqual(SyncClientState.Completed, StateUpdates[7]);
            Assert.AreEqual(SyncClientState.LoadingLocal, StateUpdates[8]);
            Assert.AreEqual(SyncClientState.Completed, StateUpdates[9]);
            
            Assert.AreEqual(8, StatusMessages.Count);
            Assert.AreEqual("TestModel SyncClient connected. Required: All (batch size 3)", StatusMessages[0]);
            Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 6. (total: 6)", StatusMessages[1]);
            Assert.AreEqual("Loading data from local store", StatusMessages[2]);
            Assert.AreEqual("Loaded 3 local models", StatusMessages[3]);
            Assert.AreEqual("0 newer TestModel entities downloaded (0 in total). 0 of 0 still to download", StatusMessages[4]);
            Assert.AreEqual("0 older TestModel entities downloaded (0 in total). 0 of 0 still to download", StatusMessages[5]);
            Assert.AreEqual("LoadMore invoked. Loading next 3 of 6 from device", StatusMessages[6]);
            Assert.AreEqual("LoadMore completed.", StatusMessages[7]);
        }

                
        [Test]
        public async Task WHEN_another_batch_does_not_exist_locally_SHOULD_load_and_publish_fromserver()
        {
            //Arrange
            var publishedModels = new List<TestModel>();
            var models = TestModel.GenerateList(6).OrderByDescending(x => x.ModifiedAtTicks).ToList();
            var localModels = models.Take(3).ToList(); 
            MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(localModels);
            MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns( 
                new ClientSyncStatus
                {
                    AllLocalEntities = 3,
                    SyncedLocalEntities = 3,
                    NewestModifiedAt = localModels.First().ModifiedAtTicks,
                    OldestModifiedAt = localModels.Last().ModifiedAtTicks
                });
            Sut.Connect(SyncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                .Subscribe(next => publishedModels.Add(next));
            publishedModels.Clear();

            var serverModels = models.Skip(3).Take(3).ToList(); 
            MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>
            {
                EntityBatch = serverModels
            });
            MockSyncCommandHandler.Mock.Invocations.Clear(); 

            //Act
            Sut.LoadNextBatch();
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

            Assert.AreEqual(10, StateUpdates.Count);
            Assert.AreEqual(SyncClientState.Starting, StateUpdates[0]);
            Assert.AreEqual(SyncClientState.Starting, StateUpdates[1]);
            Assert.AreEqual(SyncClientState.LoadingLocal, StateUpdates[2]);
            Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[3]);
            Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[4]);
            Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[5]);
            Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[6]);
            Assert.AreEqual(SyncClientState.Completed, StateUpdates[7]);
            Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[8]);
            Assert.AreEqual(SyncClientState.Completed, StateUpdates[9]);
            
            Assert.AreEqual(8, StatusMessages.Count);
            Assert.AreEqual("TestModel SyncClient connected. Required: All (batch size 3)", StatusMessages[0]);
            Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 3. (total: 3)", StatusMessages[1]);
            Assert.AreEqual("Loading data from local store", StatusMessages[2]);
            Assert.AreEqual("Loaded 3 local models", StatusMessages[3]);
            Assert.AreEqual("0 newer TestModel entities downloaded (0 in total). 0 of 0 still to download", StatusMessages[4]);
            Assert.AreEqual("0 older TestModel entities downloaded (0 in total). 0 of 0 still to download", StatusMessages[5]);
            Assert.AreEqual("LoadMore invoked. Loading next 3 of 6 from server", StatusMessages[6]);
            Assert.AreEqual("LoadMore completed.", StatusMessages[7]);
        }


    }
}