using System;
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
using Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Repositories;
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
        private TaskCompletionSource<List<SyncUpdate<TestModel>>> _tcs; 
        private ClientSyncRequirement _clientSyncRequirement;
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
            _tcs = new TaskCompletionSource<List<SyncUpdate<TestModel>>>();
            _syncCommand = new TestSyncCommand
            {
                BatchSize = 3,
                FavouriteFood = "Lasagne"
            }; 
            _clientSyncRequirement = ClientSyncRequirement.Batch;
            AddService(MockSyncClientRepository.Object);
            AddService(MockSyncCommandHandler.Object);
            AddService(MockTimeService.Object);
            AddService(MockConnectivityService.Object);
        }

        public class ApplicableToAllCases : ConnectTests
        {
            [Test]
            public void SHOULD_initialize_sync_status_and_trace()
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
                MockAnalyticsService.VerifyTrace("Initializing sync for TestModel");
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
                    _tcs.SetResult(new List<SyncUpdate<TestModel>>());
                });
                await _tcs.Task;

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
                var publishedModels = new List<SyncUpdate<TestModel>>();

                //Act
                Sut.Connect(_syncCommand, _clientSyncRequirement, MockSyncStatusHandler.Object).Subscribe(next =>
                {
                    publishedModels.Add(next);
                    if (publishedModels.Count == 3)
                    {
                        _tcs.SetResult(publishedModels);
                    }
                });
                await _tcs.Task;

                //Assert
                Assert.AreEqual(publishedModels[0].Model.Id, newServerModels[0].Id);
                Assert.AreEqual(publishedModels[1].Model.Id, newServerModels[1].Id);
                Assert.AreEqual(publishedModels[2].Model.Id, newServerModels[2].Id);
            }

            [Test]
            public async Task WHEN_SyncRequirement_is_all_SHOULD_download_and_publish_all_while_updating_client_status()
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
                var publishedModels = new List<SyncUpdate<TestModel>>();

                //Act
                Sut.Connect(_syncCommand, ClientSyncRequirement.All, MockSyncStatusHandler.Object).Subscribe(next =>
                {
                    publishedModels.Add(next);
                    if (publishedModels.Count == 9)
                    {
                        _tcs.SetResult(publishedModels);
                    }
                });
                await _tcs.Task;

                //Assert
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan == models1.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.OlderThan == models2.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(3);
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
                MockAnalyticsService.VerifyTrace("3 TestModel entities downloaded");
                MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(), Times.Exactly(4));
            }
            
            [Test]
            public async Task WHEN_SyncRequirement_has_minimum_SHOULD_download_and_publish_until_minimum_is_reached()
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
                var publishedModels = new List<SyncUpdate<TestModel>>();

                //Act
                Sut.Connect(_syncCommand, ClientSyncRequirement.AtLeast(5), MockSyncStatusHandler.Object).Subscribe(next =>
                {
                    publishedModels.Add(next);
                    if (publishedModels.Count == 6)
                    {
                        _tcs.SetResult(publishedModels);
                    }
                });
                await _tcs.Task;

                //Assert
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.OlderThan == models1.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_Times(2);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.NewlyDownloadedEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllLocalEntities = 6);
                MockSyncStatusHandler.Mock.VerifySet(x => x.AllServerEntities = 9);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 3);
                MockSyncStatusHandler.Mock.VerifySet(x => x.SyncedLocalEntities = 6);
                MockAnalyticsService.VerifyTrace("3 TestModel entities downloaded");
                MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync(), Times.Exactly(3));
            }

        }

        public class NewModelsAndMoreOldModelsAvailable : ConnectTests
        {
            private List<TestModel> _localModels;
            private List<TestModel> _newServerModels1;
            private List<TestModel> _newServerModels2;
            private List<TestModel> _oldServerModels1;
            private List<TestModel> _oldServerModels2;

            public override void Setup()
            {
                base.Setup();
                
                _localModels = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                _newServerModels1 = TestModel.GenerateList(3).OrderBy(x => x.ModifiedAtTicks).ToList();
                _newServerModels2 = TestModel.GenerateList(3).OrderBy(x => x.ModifiedAtTicks).ToList();
                _oldServerModels1 = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
                _oldServerModels2 = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();

                MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(_localModels);
                MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new List<ClientSyncStatus>
                {
                    //local entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 3,
                        SyncedLocalEntities = 3,
                        NewestModifiedAt = _localModels.First().ModifiedAtTicks,
                        OldestModifiedAt = _localModels.Last().ModifiedAtTicks
                    },
                    //after downloading first set of new entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 6,
                        SyncedLocalEntities = 6,
                        NewestModifiedAt = _newServerModels1.Last().ModifiedAtTicks,
                        OldestModifiedAt = _newServerModels1.First().ModifiedAtTicks
                    },
                    //after downloading second set of new entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 9,
                        SyncedLocalEntities = 9,
                        NewestModifiedAt = _newServerModels2.Last().ModifiedAtTicks,
                        OldestModifiedAt = _newServerModels2.First().ModifiedAtTicks
                    },
                    //after downloading first set of old entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 12,
                        SyncedLocalEntities = 12,
                        NewestModifiedAt = _oldServerModels1.First().ModifiedAtTicks,
                        OldestModifiedAt = _oldServerModels1.Last().ModifiedAtTicks
                    },
                    //after downloading second set of old entities
                    new ClientSyncStatus
                    {
                        AllLocalEntities = 15,
                        SyncedLocalEntities = 15,
                        NewestModifiedAt = _oldServerModels2.First().ModifiedAtTicks,
                        OldestModifiedAt = _oldServerModels2.Last().ModifiedAtTicks
                    },
                });
                MockSyncCommandHandler.Where_HandleAsync_returns(new List<SyncResult<TestModel>>
                {
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 6,
                        EntityBatch = _newServerModels1,
                        TotalActiveEntityCount = 200
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 3,
                        EntityBatch = _newServerModels2,
                        TotalActiveEntityCount = 200
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 6,
                        EntityBatch = _oldServerModels1,
                        TotalActiveEntityCount = 200
                    },
                    new SyncResult<TestModel>
                    {
                        EntitiesToDownloadCount = 3,
                        EntityBatch = _oldServerModels2,
                        TotalActiveEntityCount = 200
                    }
                });
            }

            [Test]
            public async Task SHOULD_load_newer_and_older_entities_from_server()
            {
                //Arrange

                //Act
                Sut.Connect(_syncCommand, _clientSyncRequirement, MockSyncStatusHandler.Object).Subscribe();

                //Assert
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == _localModels.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");

                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == _newServerModels1.First().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");
                
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == _localModels.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");
                
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.OlderThan == _oldServerModels1.Last().ModifiedAtTicks);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.NewerThan == null);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.BatchSize == 3);
                MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.FavouriteFood == "Lasagne");

            }
        }
         
    }
}