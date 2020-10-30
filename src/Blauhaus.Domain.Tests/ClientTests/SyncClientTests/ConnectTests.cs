using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.Errors;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Tests.ClientTests.SyncClientTests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.Errors;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests
{
    public class ConnectTests : BaseSyncClientTest
    {
         
        public class ApplicableToAllCases : ConnectTests
        {
            [Test]
            public void SHOULD_initialize_sync_status()
            {
                //Arrange
                var clientSyncStatus = new ClientSyncStatus
                {
                    OldestModifiedAt = 100,
                    NewestModifiedAt = 200,
                    SyncedLocalEntities = 2,
                    AllLocalEntities = 3
                };
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(clientSyncStatus);

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement, MockSyncStatusHandler.Object).Subscribe();

                //Assert
                MockBaseSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(SyncCommand));
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.IsConnected = true);
            }
        }

        public class FirstTimeSync : ConnectTests
        {
            public override void Setup()
            {
                base.Setup();

                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus
                {
                    SyncedLocalEntities = 0,
                    AllLocalEntities = 0,
                    OldestModifiedAt = 0,
                    NewestModifiedAt = 0
                });
            }

            [Test]
            public void SHOULD_load_from_server_with_modified_fields_Null()
            { 
                //Arrange
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = new List<TestModel>(),
                        EntitiesToDownloadCount = 0,
                        TotalActiveEntityCount = 0
                    }
                }); 

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement, MockSyncStatusHandler.Object).Subscribe();

                //Assert
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");
            }
            
            [Test]
            public async Task WHEN_Server_fails_SHOULD_fail_and_trace()
            { 
                //Arrange
                MockSyncCommandHandler.Where_HandleAsync_returns_fail(AuthErrors.NotAuthenticated);
                Exception e = new Exception();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement, MockSyncStatusHandler.Object).Subscribe(next =>
                { }, ex =>
                {
                    e = ex;
                });
                await Task.Delay(20);

                //Assert
                Assert.AreEqual(AuthErrors.NotAuthenticated.ToString(), e.Message);
                MockAnalyticsService.VerifyTrace("Failed to load TestModel entities from server: The current user has not been successfully authenticated", LogSeverity.Error);
                MockSyncStatusHandler.Mock.VerifySet(x => x.StatusMessage = "Failed to load TestModel entities from server: The current user has not been successfully authenticated");
            }

            [Test]
            public async Task WHEN_Server_fails_with_Error_SHOULD_fail_and_trace()
            { 
                //Arrange
                MockSyncCommandHandler.Where_HandleAsync_returns_fail(AuthErrors.NotAuthenticated);
                Exception e = new Exception();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement, MockSyncStatusHandler.Object).Subscribe(next =>
                    { }, ex =>
                    {
                        e = ex;
                    });
                await Task.Delay(20);

                //Assert
                Assert.That(e, Is.InstanceOf<ErrorException>());
                Assert.That(((ErrorException)e).Error, Is.EqualTo(AuthErrors.NotAuthenticated));
                MockAnalyticsService.VerifyTrace("Failed to load TestModel entities from server: " + AuthErrors.NotAuthenticated.Description, LogSeverity.Error);
                MockSyncStatusHandler.Mock.VerifySet(x => x.StatusMessage = "Failed to load TestModel entities from server: " + AuthErrors.NotAuthenticated.Description);
            }

            [Test]
            public async Task WHEN_Server_returns_models_SHOULD_publish()
            { 
                //Arrange
                var newServerModels = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>
                {
                    EntityBatch = newServerModels,
                    EntitiesToDownloadCount = newServerModels.Count,
                    TotalActiveEntityCount = 300
                });
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement, MockSyncStatusHandler.Object).Subscribe(next =>
                {
                    publishedModels.Add(next); 
                });
                await Task.Delay(20);

                //Assert
                Assert.AreEqual(publishedModels[0].Id, newServerModels[0].Id);
                Assert.AreEqual(publishedModels[1].Id, newServerModels[1].Id);
                Assert.AreEqual(publishedModels[2].Id, newServerModels[2].Id);
                Assert.AreEqual(3, newServerModels.Count);
            }

            [Test]
            public async Task WHEN_SyncRequirement_is_Batch_SHOULD_download_and_publish_only_one_batch()
            { 
                //Arrange
                var models1 = TestModel.GenerateList(3).ToList();
                var models2 = TestModel.GenerateList(3).ToList();
                var models3 = TestModel.GenerateList(3).ToList();
                MockSyncStatusHandler.Mock.SetupAllProperties();
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models1,
                        EntitiesToDownloadCount = 9,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models2,
                        EntitiesToDownloadCount = 6,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models3,
                        EntitiesToDownloadCount = 3,
                        TotalActiveEntityCount = 9
                    }
                });
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 0,
                        AllLocalEntities = 0,
                        OldestModifiedAt = 0,
                        NewestModifiedAt = 0
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 3,
                        AllLocalEntities = 3,
                        OldestModifiedAt = models1.Last().ModifiedAtTicks,
                        NewestModifiedAt = models1.First().ModifiedAtTicks
                    },                    
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 6,
                        AllLocalEntities = 6,
                        OldestModifiedAt = models2.Last().ModifiedAtTicks,
                        NewestModifiedAt = models2.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 9,
                        AllLocalEntities = 9,
                        OldestModifiedAt = models3.Last().ModifiedAtTicks,
                        NewestModifiedAt = models3.First().ModifiedAtTicks
                    }});
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.Batch, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(20);

                //Assert
                Assert.AreEqual(3, publishedModels.Count);
                Assert.AreEqual(models1[0].Id, publishedModels[0].Id);
                Assert.AreEqual(models1[1].Id, publishedModels[1].Id);
                Assert.AreEqual(models1[2].Id, publishedModels[2].Id);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null); 
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllServerEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.TotalEntitiesToDownload = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockBaseSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(SyncCommand), Times.Exactly(2));
            }
            
            [Test]
            public async Task WHEN_SyncRequirement_is_Batch_SHOULD_trace_status_messages()
            { 
                //Arrange
                var serverModels = TestModel.GenerateList(9);
                var models1 = serverModels.Skip(0).Take(3).ToList();
                var models2 = serverModels.Skip(3).Take(3).ToList();
                var models3 = serverModels.Skip(6).Take(3).ToList();
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models1,
                        EntitiesToDownloadCount = 9,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models2,
                        EntitiesToDownloadCount = 6,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models3,
                        EntitiesToDownloadCount = 3,
                        TotalActiveEntityCount = 9
                    }
                });
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 0,
                        AllLocalEntities = 0,
                        OldestModifiedAt = 0,
                        NewestModifiedAt = 0
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 3,
                        AllLocalEntities = 3,
                        OldestModifiedAt = models1.Last().ModifiedAtTicks,
                        NewestModifiedAt = models1.First().ModifiedAtTicks
                    },                    
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 6,
                        AllLocalEntities = 6,
                        OldestModifiedAt = models2.Last().ModifiedAtTicks,
                        NewestModifiedAt = models2.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 9,
                        AllLocalEntities = 9,
                        OldestModifiedAt = models3.Last().ModifiedAtTicks,
                        NewestModifiedAt = models3.First().ModifiedAtTicks
                    }});
                var publishedModels = new List<TestModel>();


                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.Batch, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(20);

                //Assert
                Assert.AreEqual("TestModel SyncClient connected. Required: Batch (batch size 3)", StatusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 0. (total: 0)", StatusMessages[1]);
                Assert.AreEqual("No local data, checking server...", StatusMessages[2]);
                Assert.AreEqual("3 older TestModel entities downloaded (3 in total). 0 of 9 still to download", StatusMessages[3]);
                Assert.AreEqual(5, StateUpdates.Count);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[1]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[3]);
                Assert.AreEqual(SyncClientState.Completed, StateUpdates[4]);
            }

            [Test]
            public void WHEN_SyncRequirement_is_All_SHOULD_download_all_but_publish_only_one_batch()
            {
                //Arrange
                var serverModels = TestModel.GenerateList(9);
                var models1 = serverModels.Skip(0).Take(3).ToList();
                var models2 = serverModels.Skip(3).Take(3).ToList();
                var models3 = serverModels.Skip(6).Take(3).ToList();
                MockSyncStatusHandler.Mock.SetupAllProperties();
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models1,
                        EntitiesToDownloadCount = 9,
                        TotalActiveEntityCount = 9
                    },
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models2,
                        EntitiesToDownloadCount = 6,
                        TotalActiveEntityCount = 9
                    },
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models3,
                        EntitiesToDownloadCount = 3,
                        TotalActiveEntityCount = 9
                    }
                });
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 0,
                        AllLocalEntities = 0,
                        OldestModifiedAt = 0,
                        NewestModifiedAt = 0
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 3,
                        AllLocalEntities = 3,
                        OldestModifiedAt = models1.Last().ModifiedAtTicks,
                        NewestModifiedAt = models1.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 6,
                        AllLocalEntities = 6,
                        OldestModifiedAt = models2.Last().ModifiedAtTicks,
                        NewestModifiedAt = models2.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 9,
                        AllLocalEntities = 9,
                        OldestModifiedAt = models3.Last().ModifiedAtTicks,
                        NewestModifiedAt = models3.First().ModifiedAtTicks
                    }});
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));

                //Assert
                Assert.AreEqual(3, publishedModels.Count);
                Assert.AreEqual(models1[0].Id, publishedModels[0].Id);
                Assert.AreEqual(models1[1].Id, publishedModels[1].Id);
                Assert.AreEqual(models1[2].Id, publishedModels[2].Id);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan == models1.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.OlderThan == models2.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 6, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 9, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllServerEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.TotalEntitiesToDownload = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 9);
                MockBaseSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(SyncCommand), Times.Exactly(4));
            }

            [Test]
            public void WHEN_SyncRequirement_is_All_SHOULD_trace_status_messages()
            {
                //Arrange
                var models1 = TestModel.GenerateList(3).ToList();
                var models2 = TestModel.GenerateList(3).ToList();
                var models3 = TestModel.GenerateList(3).ToList();
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models1,
                        EntitiesToDownloadCount = 9,
                        TotalActiveEntityCount = 9
                    },
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models2,
                        EntitiesToDownloadCount = 6,
                        TotalActiveEntityCount = 9
                    },
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models3,
                        EntitiesToDownloadCount = 3,
                        TotalActiveEntityCount = 9
                    }
                });
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 0,
                        AllLocalEntities = 0,
                        OldestModifiedAt = 0,
                        NewestModifiedAt = 0
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 3,
                        AllLocalEntities = 3,
                        OldestModifiedAt = models1.Last().ModifiedAtTicks,
                        NewestModifiedAt = models1.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 6,
                        AllLocalEntities = 6,
                        OldestModifiedAt = models2.Last().ModifiedAtTicks,
                        NewestModifiedAt = models2.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 9,
                        AllLocalEntities = 9,
                        OldestModifiedAt = models3.Last().ModifiedAtTicks,
                        NewestModifiedAt = models3.First().ModifiedAtTicks
                    }});
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));

                //Assert
                Assert.AreEqual("TestModel SyncClient connected. Required: All (batch size 3)", StatusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 0. (total: 0)", StatusMessages[1]);
                Assert.AreEqual("No local data, checking server...", StatusMessages[2]);
                Assert.AreEqual("3 older TestModel entities downloaded (3 in total). 6 of 9 still to download", StatusMessages[3]);
                Assert.AreEqual("3 older TestModel entities downloaded (6 in total). 3 of 9 still to download", StatusMessages[4]);
                Assert.AreEqual("3 older TestModel entities downloaded (9 in total). 0 of 9 still to download", StatusMessages[5]);
                Assert.AreEqual(7, StateUpdates.Count);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[1]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[3]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[4]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[5]);
                Assert.AreEqual(SyncClientState.Completed, StateUpdates[6]);
            }

            [Test]
            public async Task WHEN_SyncRequirement_has_minimum_SHOULD_download_until_minimum_is_reached_but_publish_only_batch()
            { 
                //Arrange
                var serverModels = TestModel.GenerateList(9);
                var models1 = serverModels.Skip(0).Take(3).ToList();
                var models2 = serverModels.Skip(3).Take(3).ToList();
                var models3 = serverModels.Skip(6).Take(3).ToList();
                MockSyncStatusHandler.Mock.SetupAllProperties();
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models1,
                        EntitiesToDownloadCount = 9,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models2,
                        EntitiesToDownloadCount = 6,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models3,
                        EntitiesToDownloadCount = 3,
                        TotalActiveEntityCount = 9
                    }
                });
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 0,
                        AllLocalEntities = 0,
                        OldestModifiedAt = 0,
                        NewestModifiedAt = 0
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 3,
                        AllLocalEntities = 3,
                        OldestModifiedAt = models1.Last().ModifiedAtTicks,
                        NewestModifiedAt = models1.First().ModifiedAtTicks
                    },                    
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 6,
                        AllLocalEntities = 6,
                        OldestModifiedAt = models2.Last().ModifiedAtTicks,
                        NewestModifiedAt = models2.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 9,
                        AllLocalEntities = 9,
                        OldestModifiedAt = models3.Last().ModifiedAtTicks,
                        NewestModifiedAt = models3.First().ModifiedAtTicks
                    }});
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.Minimum(5), MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(20);

                //Assert
                Assert.AreEqual(3, publishedModels.Count);
                Assert.AreEqual(models1[0].Id, publishedModels[0].Id);
                Assert.AreEqual(models1[1].Id, publishedModels[1].Id);
                Assert.AreEqual(models1[2].Id, publishedModels[2].Id);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan == models1.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllServerEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 6);
                MockBaseSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(SyncCommand), Times.Exactly(3));
            }

            [Test]
            public async Task WHEN_SyncRequirement_has_minimum_SHOULD_update_status_messages()
            { 
                //Arrange
                var models1 = TestModel.GenerateList(3).ToList();
                var models2 = TestModel.GenerateList(3).ToList();
                var models3 = TestModel.GenerateList(3).ToList(); 
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models1,
                        EntitiesToDownloadCount = 9,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models2,
                        EntitiesToDownloadCount = 6,
                        TotalActiveEntityCount = 9
                    },                    
                    new SyncResult<TestModel>
                    {
                        EntityBatch = models3,
                        EntitiesToDownloadCount = 3,
                        TotalActiveEntityCount = 9
                    }
                });
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 0,
                        AllLocalEntities = 0,
                        OldestModifiedAt = 0,
                        NewestModifiedAt = 0
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 3,
                        AllLocalEntities = 3,
                        OldestModifiedAt = models1.Last().ModifiedAtTicks,
                        NewestModifiedAt = models1.First().ModifiedAtTicks
                    },                    
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 6,
                        AllLocalEntities = 6,
                        OldestModifiedAt = models2.Last().ModifiedAtTicks,
                        NewestModifiedAt = models2.First().ModifiedAtTicks
                    },
                    new ClientSyncStatus
                    {
                        SyncedLocalEntities = 9,
                        AllLocalEntities = 9,
                        OldestModifiedAt = models3.Last().ModifiedAtTicks,
                        NewestModifiedAt = models3.First().ModifiedAtTicks
                    }});

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.Minimum(5), MockSyncStatusHandler.Object)
                    .Subscribe(next => {});
                await Task.Delay(20);

                //Assert
                Assert.AreEqual("TestModel SyncClient connected. Required: Minimum 5 (batch size 3)", StatusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 0. (total: 0)", StatusMessages[1]);
                Assert.AreEqual("No local data, checking server...", StatusMessages[2]);
                Assert.AreEqual("3 older TestModel entities downloaded (3 in total). 2 of 9 still to download", StatusMessages[3]);
                Assert.AreEqual("3 older TestModel entities downloaded (6 in total). 0 of 9 still to download", StatusMessages[4]);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[1]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[3]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[4]);
                Assert.AreEqual(SyncClientState.Completed, StateUpdates[5]);
                Assert.AreEqual(6, StateUpdates.Count);
            }

        }

        public class NotFirstTimeSync : ConnectTests
        { 
            [Test]
            public async Task WHEN_SyncRequirement_All_SHOULD_load_local_and_newer_and_older_entities_from_server_but_publish_only_local_and_new_server()
            {
                 
                //Arrange
                var now = DateTime.UtcNow;
                var localModels = TestModel.GenerateOlderThan(now, 3);
                var newServerModels = TestModel.GenerateNewerThan(now, 6);
                var newServerModels1 = newServerModels.Skip(0).Take(3).ToList();
                var newServerModels2 = newServerModels.Skip(3).Take(3).ToList();
                var oldServerModels = TestModel.GenerateOlderThan(now.AddDays(-12), 6);
                var oldServerModels1 = oldServerModels.Skip(0).Take(3).ToList();
                var oldServerModels2 = oldServerModels.Skip(3).Take(3).ToList();

                MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(localModels);
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    //local entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 3,
                        SyncedLocalEntities = 3,
                        NewestModifiedAt = localModels.First().ModifiedAtTicks,
                        OldestModifiedAt = localModels.Last().ModifiedAtTicks
                    },
                    //after downloading first set of new entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 6,
                        SyncedLocalEntities = 6,
                        NewestModifiedAt = newServerModels1.Last().ModifiedAtTicks,
                        OldestModifiedAt = newServerModels1.First().ModifiedAtTicks
                    },
                    //after downloading second set of new entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 9,
                        SyncedLocalEntities = 9,
                        NewestModifiedAt = newServerModels2.Last().ModifiedAtTicks,
                        OldestModifiedAt = newServerModels2.First().ModifiedAtTicks
                    },
                    //after downloading first set of old entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 12,
                        SyncedLocalEntities = 12,
                        NewestModifiedAt = oldServerModels1.First().ModifiedAtTicks,
                        OldestModifiedAt = oldServerModels1.Last().ModifiedAtTicks
                    },
                    //after downloading second set of old entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 15,
                        SyncedLocalEntities = 15,
                        NewestModifiedAt = oldServerModels2.First().ModifiedAtTicks,
                        OldestModifiedAt = oldServerModels2.Last().ModifiedAtTicks
                    },
                });
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 6,
                        EntityBatch = newServerModels1,
                        TotalActiveEntityCount = 15
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 3,
                        EntityBatch = newServerModels2,
                        TotalActiveEntityCount = 15
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 6,
                        EntityBatch = oldServerModels1,
                        TotalActiveEntityCount = 15
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 3,
                        EntityBatch = oldServerModels2,
                        TotalActiveEntityCount = 15
                    }
                });
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(20);

                //Assert
                MockBaseSyncClientRepository.Mock.Verify(x => x.LoadModelsAsync(SyncCommand));
                Assert.AreEqual(localModels[0].Id, publishedModels[0].Id);
                Assert.AreEqual(localModels[1].Id, publishedModels[1].Id);
                Assert.AreEqual(localModels[2].Id, publishedModels[2].Id);
                Assert.AreEqual(newServerModels1[0].Id, publishedModels[3].Id);
                Assert.AreEqual(newServerModels1[1].Id, publishedModels[4].Id);
                Assert.AreEqual(newServerModels1[2].Id, publishedModels[5].Id);
                Assert.AreEqual(newServerModels2[0].Id, publishedModels[6].Id);
                Assert.AreEqual(newServerModels2[1].Id, publishedModels[7].Id);
                Assert.AreEqual(newServerModels2[2].Id, publishedModels[8].Id);
                Assert.AreEqual(9, publishedModels.Count);
                
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 10, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 12);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 15, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 12);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 15);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllServerEntities = 15);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 12);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 15);

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == localModels.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == newServerModels1.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.OlderThan == localModels.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(3, x => x.OlderThan == oldServerModels1.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(3, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(3, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(3, x => x.FavouriteFood == "Lasagne");
            }

            [Test]
            public void IF_SyncRequirement_is_All_SHOULD_check_for_older_items_even_when_all_synced()
            {
                //Arrange
                var localModels = TestModel.GenerateList(15).OrderByDescending(x => x.ModifiedAtTicks).ToList(); 

                MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(localModels);
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus
                {
                    AllLocalEntities = 15,
                    SyncedLocalEntities = 15,
                    NewestModifiedAt = localModels.First().ModifiedAtTicks,
                    OldestModifiedAt = localModels.Last().ModifiedAtTicks
                });
                MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>
                {
                    EntitiesToDownloadCount = 0,
                    EntityBatch = new List<TestModel>(),
                    TotalActiveEntityCount = 15
                });

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object).Subscribe();

                //Assert
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == localModels.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan ==  localModels.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.FavouriteFood == "Lasagne");
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(2);
            }

            [Test]
            public void IF_SyncRequirement_is_AtLeast_and_minimum_has_been_downloaded_SHOULD_not_check_for_older_items()
            {
                //Arrange
                var localModels = TestModel.GenerateList(15).OrderByDescending(x => x.ModifiedAtTicks).ToList(); 

                MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(localModels);
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus
                {
                    AllLocalEntities = 15,
                    SyncedLocalEntities = 15,
                    NewestModifiedAt = localModels.First().ModifiedAtTicks,
                    OldestModifiedAt = localModels.Last().ModifiedAtTicks
                });
                MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>
                {
                    EntitiesToDownloadCount = 0,
                    EntityBatch = new List<TestModel>(),
                    TotalActiveEntityCount = 15
                });

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.Minimum(15), MockSyncStatusHandler.Object).Subscribe();

                //Assert
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == localModels.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(1);
            }
            
            [Test]
            public async Task WHEN_SyncRequirement_Minimum_SHOULD_load_all_newer_and_older_entities_from_server_until_mnimum_reached()
            {
                 //Arrange
                 var now = DateTime.UtcNow;
                 var localModels = TestModel.GenerateOlderThan(now, 3);
                 var newServerModels = TestModel.GenerateNewerThan(now, 6);
                 var newServerModels1 = newServerModels.Skip(0).Take(3).ToList();
                 var newServerModels2 = newServerModels.Skip(3).Take(3).ToList();
                 var oldServerModels = TestModel.GenerateOlderThan(now.AddDays(-12), 6);
                 var oldServerModels1 = oldServerModels.Skip(0).Take(3).ToList();
                 var oldServerModels2 = oldServerModels.Skip(3).Take(3).ToList();
                MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(localModels);
                MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    //local entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 3,
                        SyncedLocalEntities = 3,
                        NewestModifiedAt = localModels.First().ModifiedAtTicks,
                        OldestModifiedAt = localModels.Last().ModifiedAtTicks
                    },
                    //after downloading first set of new entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 6,
                        SyncedLocalEntities = 6,
                        NewestModifiedAt = newServerModels1.Last().ModifiedAtTicks,
                        OldestModifiedAt = newServerModels1.First().ModifiedAtTicks
                    },
                    //after downloading second set of new entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 9,
                        SyncedLocalEntities = 9,
                        NewestModifiedAt = newServerModels2.Last().ModifiedAtTicks,
                        OldestModifiedAt = newServerModels2.First().ModifiedAtTicks
                    },
                    //after downloading first set of old entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 12,
                        SyncedLocalEntities = 12,
                        NewestModifiedAt = oldServerModels1.First().ModifiedAtTicks,
                        OldestModifiedAt = oldServerModels1.Last().ModifiedAtTicks
                    },
                    //after downloading second set of old entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 15,
                        SyncedLocalEntities = 15,
                        NewestModifiedAt = oldServerModels2.First().ModifiedAtTicks,
                        OldestModifiedAt = oldServerModels2.Last().ModifiedAtTicks
                    },
                });
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 6,
                        EntityBatch = newServerModels1,
                        TotalActiveEntityCount = 15
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 3,
                        EntityBatch = newServerModels2,
                        TotalActiveEntityCount = 15
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 6,
                        EntityBatch = oldServerModels1,
                        TotalActiveEntityCount = 15
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 3,
                        EntityBatch = oldServerModels2,
                        TotalActiveEntityCount = 15
                    }
                });
                var publishedModels = new List<TestModel>();

                //Act
                Sut.Connect(SyncCommand, ClientSyncRequirement.Minimum(11), MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(10);

                //Assert
                MockBaseSyncClientRepository.Mock.Verify(x => x.LoadModelsAsync(SyncCommand));
                Assert.AreEqual(localModels[0].Id, publishedModels[0].Id);
                Assert.AreEqual(localModels[1].Id, publishedModels[1].Id);
                Assert.AreEqual(localModels[2].Id, publishedModels[2].Id);
                Assert.AreEqual(newServerModels1[0].Id, publishedModels[3].Id);
                Assert.AreEqual(newServerModels1[1].Id, publishedModels[4].Id);
                Assert.AreEqual(newServerModels1[2].Id, publishedModels[5].Id);
                Assert.AreEqual(newServerModels2[0].Id, publishedModels[6].Id);
                Assert.AreEqual(newServerModels2[1].Id, publishedModels[7].Id);
                Assert.AreEqual(newServerModels2[2].Id, publishedModels[8].Id);
                Assert.AreEqual(9, publishedModels.Count);

                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 10, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 12, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 15, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 12);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 15, Times.Never);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllServerEntities = 15);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 12);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 15, Times.Never);

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == localModels.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == newServerModels1.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.OlderThan == localModels.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.FavouriteFood == "Lasagne");
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(3);
                
                Assert.AreEqual("TestModel SyncClient connected. Required: Minimum 11 (batch size 3)", StatusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 3. (total: 3)", StatusMessages[1]);
                Assert.AreEqual("Loading data from local store", StatusMessages[2]);
                Assert.AreEqual("Loaded 3 local models", StatusMessages[3]);
                Assert.AreEqual("3 newer TestModel entities downloaded (3 in total). 3 of 15 still to download", StatusMessages[4]);
                Assert.AreEqual("3 newer TestModel entities downloaded (6 in total). 0 of 15 still to download", StatusMessages[5]);
                Assert.AreEqual("3 older TestModel entities downloaded (9 in total). 0 of 15 still to download", StatusMessages[6]);
                Assert.AreEqual(9, StateUpdates.Count);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, StateUpdates[1]);
                Assert.AreEqual(SyncClientState.LoadingLocal, StateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[3]);
                Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[4]);
                Assert.AreEqual(SyncClientState.DownloadingNew, StateUpdates[5]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[6]);
                Assert.AreEqual(SyncClientState.DownloadingOld, StateUpdates[7]);
                Assert.AreEqual(SyncClientState.Completed, StateUpdates[8]);
            }

        }
         
    }
}