using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Tests.ClientTests.SyncClientTests._Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests
{
    public class ReloadFromClientTests : BaseSyncClientTest
    {
                
        [Test]
        public async Task SHOULD_load_and_publish_all_models_from_client()
        {
            //Arrange
            var publishedModels = new List<TestModel>();
            var now = DateTime.UtcNow;
            var localModels = TestModel.GenerateOlderThan(now, 3); 
            var newLocalModels = TestModel.GenerateNewerThan(now, 3); 
            var invokedWithCommands = MockBaseSyncClientRepository.Where_LoadModelsAsync_returns_sequence(new List<List<TestModel>>{ localModels, newLocalModels });
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
            MockSyncCommandHandler.Mock.Invocations.Clear(); 

            //Act
            Sut.ReloadFromClient();
            await Task.Delay(20);

            //Assert
            Assert.AreEqual("Lasagne", invokedWithCommands[0].FavouriteFood);
            Assert.AreEqual(3, invokedWithCommands[0].BatchSize);
            Assert.AreEqual(null, invokedWithCommands[0].NewerThan);
            Assert.AreEqual(null, invokedWithCommands[0].OlderThan);
            Assert.AreEqual("Lasagne", invokedWithCommands[1].FavouriteFood);
            Assert.AreEqual(3, invokedWithCommands[1].BatchSize);
            Assert.AreEqual(null, invokedWithCommands[1].NewerThan);
            Assert.AreEqual(null, invokedWithCommands[1].OlderThan);
            Assert.AreEqual(3, publishedModels.Count);
            Assert.AreEqual(newLocalModels[0].Id, publishedModels[0].Id);
            Assert.AreEqual(newLocalModels[1].Id, publishedModels[1].Id);
            Assert.AreEqual(newLocalModels[2].Id, publishedModels[2].Id);

            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
            MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4, Times.Never);

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
            Assert.AreEqual("SyncClient connected. Required: All (batch size 3)", StatusMessages[0]);
            Assert.AreEqual("Initializing sync. Local status Synced: 3. (total: 3)", StatusMessages[1]);
            Assert.AreEqual("Loading data from local store", StatusMessages[2]);
            Assert.AreEqual("Loaded 3 local models", StatusMessages[3]);
            Assert.AreEqual("0 newer TestModel entities downloaded (0 in total). 0 of 0 still to download", StatusMessages[4]);
            Assert.AreEqual("0 older TestModel entities downloaded (0 in total). 0 of 0 still to download", StatusMessages[5]);
            Assert.AreEqual("Reload from client invoked. Loading all models from local store", StatusMessages[6]);
            Assert.AreEqual("Reload from client completed. 3 loaded", StatusMessages[7]); 
        }


    }
}