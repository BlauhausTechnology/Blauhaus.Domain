﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Time.Service;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.Domain.Tests.ServerTests.TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using TestSyncCommand = Blauhaus.Domain.Tests.ClientTests.TestObjects.TestSyncCommand;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests
{
    public class ConnectTests : BaseDomainTest<SyncClient<TestModel, TestModelDto, TestSyncCommand>>
    {
        private TestSyncCommand _syncCommand; 
        private ClientSyncRequirement _clientSyncRequirement;
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

            MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>{EntityBatch = new List<TestModel>()});
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(clientSyncStatus);

                //Act
                Sut.Connect(_syncCommand, _clientSyncRequirement, MockSyncStatusHandler.Object).Subscribe();

                //Assert
                MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync());
                MockAnalyticsService.VerifyTraceProperty(nameof(ClientSyncStatus), clientSyncStatus);
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

                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus
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
                Sut.Connect(_syncCommand, _clientSyncRequirement, MockSyncStatusHandler.Object).Subscribe();

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
                MockSyncCommandHandler.Where_HandleAsync_returns_fail("oops");
                Exception e = new Exception();

                //Act
                Sut.Connect(_syncCommand, _clientSyncRequirement, MockSyncStatusHandler.Object).Subscribe(next =>
                { }, ex =>
                {
                    e = ex;
                });
                await Task.Delay(20);

                //Assert
                Assert.AreEqual("Failed to load TestModel entities from server: oops", e.Message);
                MockAnalyticsService.VerifyTrace("Failed to load TestModel entities from server: oops", LogSeverity.Error);
                MockSyncStatusHandler.Mock.VerifySet(x => x.StatusMessage = "Failed to load TestModel entities from server: oops");
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
                Sut.Connect(_syncCommand, _clientSyncRequirement, MockSyncStatusHandler.Object).Subscribe(next =>
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.Batch, MockSyncStatusHandler.Object)
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
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(), Times.Exactly(2));
            }
            
            [Test]
            public async Task WHEN_SyncRequirement_is_Batch_SHOULD_trace_status_messages()
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.Batch, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(20);

                //Assert
                Assert.AreEqual("TestModel SyncClient connected. Required: Batch (batch size 3)", _statusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 0. (total: 0)", _statusMessages[1]);
                Assert.AreEqual("No local data, checking server...", _statusMessages[2]);
                Assert.AreEqual("3 older TestModel entities downloaded (3 in total). 0 of 9 still to download", _statusMessages[3]);
                Assert.AreEqual(5, _stateUpdates.Count);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[1]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[3]);
                Assert.AreEqual(SyncClientState.Completed, _stateUpdates[4]);
            }

            [Test]
            public void WHEN_SyncRequirement_is_All_SHOULD_download_all_but_publish_only_one_batch()
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
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
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 9);
                MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(), Times.Exactly(4));
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));

                //Assert
                Assert.AreEqual("TestModel SyncClient connected. Required: All (batch size 3)", _statusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 0. (total: 0)", _statusMessages[1]);
                Assert.AreEqual("No local data, checking server...", _statusMessages[2]);
                Assert.AreEqual("3 older TestModel entities downloaded (3 in total). 6 of 9 still to download", _statusMessages[3]);
                Assert.AreEqual("3 older TestModel entities downloaded (6 in total). 3 of 9 still to download", _statusMessages[4]);
                Assert.AreEqual("3 older TestModel entities downloaded (9 in total). 0 of 9 still to download", _statusMessages[5]);
                Assert.AreEqual(7, _stateUpdates.Count);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[1]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[3]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[4]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[5]);
                Assert.AreEqual(SyncClientState.Completed, _stateUpdates[6]);
            }

            [Test]
            public async Task WHEN_SyncRequirement_has_minimum_SHOULD_download_until_minimum_is_reached_but_publish_only_batch()
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.Minimum(5), MockSyncStatusHandler.Object)
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
                MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(), Times.Exactly(3));
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
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.Minimum(5), MockSyncStatusHandler.Object)
                    .Subscribe(next => {});
                await Task.Delay(20);

                //Assert
                Assert.AreEqual("TestModel SyncClient connected. Required: Minimum 5 (batch size 3)", _statusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 0. (total: 0)", _statusMessages[1]);
                Assert.AreEqual("No local data, checking server...", _statusMessages[2]);
                Assert.AreEqual("3 older TestModel entities downloaded (3 in total). 2 of 9 still to download", _statusMessages[3]);
                Assert.AreEqual("3 older TestModel entities downloaded (6 in total). 0 of 9 still to download", _statusMessages[4]);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[1]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[3]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[4]);
                Assert.AreEqual(SyncClientState.Completed, _stateUpdates[5]);
                Assert.AreEqual(6, _stateUpdates.Count);
            }

        }

        public class NotFirstTimeSync : ConnectTests
        { 
            [Test]
            public async Task WHEN_SyncRequirement_All_SHOULD_load_all_local_and_all_newer_and_older_entities_from_server_but_publish_only_batch()
            {
                 //Arrange
                var localModels = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                var newServerModels1 = TestModel.GenerateList(3).OrderBy(x => x.ModifiedAtTicks).ToList();
                var newServerModels2 = TestModel.GenerateList(3).OrderBy(x => x.ModifiedAtTicks).ToList();
                var oldServerModels1 = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                var oldServerModels2 = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();

                MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(localModels);
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(20);

                //Assert
                MockSyncClientRepository.Mock.Verify(x => x.LoadModelsAsync(_syncCommand));
                Assert.AreEqual(3, publishedModels.Count);
                Assert.AreEqual(localModels[0].Id, publishedModels[0].Id);
                Assert.AreEqual(localModels[1].Id, publishedModels[1].Id);
                Assert.AreEqual(localModels[2].Id, publishedModels[2].Id);
                
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4, Times.Never);
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

                MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(localModels);
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object).Subscribe();

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

                MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(localModels);
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.Minimum(15), MockSyncStatusHandler.Object).Subscribe();

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
                var localModels = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                var newServerModels1 = TestModel.GenerateList(3).OrderBy(x => x.ModifiedAtTicks).ToList();
                var newServerModels2 = TestModel.GenerateList(3).OrderBy(x => x.ModifiedAtTicks).ToList();
                var oldServerModels1 = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                var oldServerModels2 = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList(); 
                MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(localModels);
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
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
                Sut.Connect(_syncCommand, ClientSyncRequirement.Minimum(11), MockSyncStatusHandler.Object)
                    .Subscribe(next => publishedModels.Add(next));
                await Task.Delay(10);

                //Assert
                MockSyncClientRepository.Mock.Verify(x => x.LoadModelsAsync(_syncCommand));
                Assert.AreEqual(3, publishedModels.Count);
                Assert.AreEqual(localModels[0].Id, publishedModels[0].Id);
                Assert.AreEqual(localModels[1].Id, publishedModels[1].Id);
                Assert.AreEqual(localModels[2].Id, publishedModels[2].Id);

                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 1);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.PublishedEntities = 4, Times.Never);
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
                
                Assert.AreEqual("TestModel SyncClient connected. Required: Minimum 11 (batch size 3)", _statusMessages[0]);
                Assert.AreEqual("Initializing sync for TestModel. Local status Synced: 3. (total: 3)", _statusMessages[1]);
                Assert.AreEqual("Loading data from local store", _statusMessages[2]);
                Assert.AreEqual("Loaded 3 local models", _statusMessages[3]);
                Assert.AreEqual("3 newer TestModel entities downloaded (3 in total). 3 of 15 still to download", _statusMessages[4]);
                Assert.AreEqual("3 newer TestModel entities downloaded (6 in total). 0 of 15 still to download", _statusMessages[5]);
                Assert.AreEqual("3 older TestModel entities downloaded (9 in total). 0 of 15 still to download", _statusMessages[6]);
                Assert.AreEqual(9, _stateUpdates.Count);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[0]);
                Assert.AreEqual(SyncClientState.Starting, _stateUpdates[1]);
                Assert.AreEqual(SyncClientState.LoadingLocal, _stateUpdates[2]);
                Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[3]);
                Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[4]);
                Assert.AreEqual(SyncClientState.DownloadingNew, _stateUpdates[5]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[6]);
                Assert.AreEqual(SyncClientState.DownloadingOld, _stateUpdates[7]);
                Assert.AreEqual(SyncClientState.Completed, _stateUpdates[8]);
            }

        }
         
    }
}