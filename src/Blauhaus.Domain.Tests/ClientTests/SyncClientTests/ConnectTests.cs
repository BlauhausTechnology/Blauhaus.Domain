using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Repositories._Base;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.Domain.Tests.ServerTests.TestObjects;
using Moq;
using NUnit.Framework;
using TestSyncCommand = Blauhaus.Domain.Tests.ClientTests.TestObjects.TestSyncCommand;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests
{
    public class ConnectTests : BaseDomainTest<SyncClient<TestModel, TestModelDto, TestSyncCommand>>
    {
        private TestSyncCommand _syncCommand;
        private List<TestModel> _localModels;
        private List<TestModel> _serverModels;
        private TaskCompletionSource<List<SyncUpdate<TestModel>>> _tcs;
        private ClientSyncStatus _localSyncstatus;
        private SyncResult<TestModel> _syncResult;

        private SyncClientRepositoryMockBuilder<ISyncClientRepository<TestModel, TestModelDto>, TestModel, TestModelDto> MockSyncClientRepository 
            => AddMock<SyncClientRepositoryMockBuilder<ISyncClientRepository<TestModel, TestModelDto>, TestModel, TestModelDto>, ISyncClientRepository<TestModel, TestModelDto>>().Invoke();

        private CommandHandlerMockBuilder<SyncResult<TestModel>, TestSyncCommand> MockSyncCommandHandler 
            => AddMock<CommandHandlerMockBuilder<SyncResult<TestModel>, TestSyncCommand>, ICommandHandler<SyncResult<TestModel>, TestSyncCommand>>().Invoke();

        public override void Setup()
        {
            base.Setup();
            _tcs = new TaskCompletionSource<List<SyncUpdate<TestModel>>>();
            _serverModels = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
            _localModels = TestModel.GenerateList(3).OrderByDescending(x => x.ModifiedAtTicks).ToList();
            _syncCommand = new TestSyncCommand{BatchSize = 12};
            _localSyncstatus = new ClientSyncStatus
            {
                TotalCount = 3,
                LastModifiedAt = _localModels.First().ModifiedAtTicks,
                FirstModifiedAt = _localModels.Last().ModifiedAtTicks
            };
            _syncResult = new SyncResult<TestModel>
            {
                Entities = _serverModels,
                ModifiedEntityCount = 30,
                TotalEntityCount = 300
            };

            MockSyncClientRepository.Where_LoadSyncedModelsAsync_returns(_localModels);
            MockSyncClientRepository.Where_GetSyncStatusAsync_returns(_localSyncstatus);
            MockSyncCommandHandler.Where_HandleAsync_returns(_syncResult);
            
            AddService(MockSyncClientRepository.Object);
            AddService(MockSyncCommandHandler.Object);
        }

        
        [Test]
        public void SHOULD_trace()
        {
            //Act
            Sut.Connect(_syncCommand).Subscribe();

            //Assert
            MockAnalyticsService.VerifyTrace("TestModel SyncClient connected");
        }

        [Test]
        public void SHOULD_initialize_sync_status_and_load_local_entities()
        {
            //Act
            Sut.Connect(_syncCommand).Subscribe();

            //Assert
            MockSyncClientRepository.Mock.Verify(x => x.GetSyncStatusAsync());
            MockSyncClientRepository.Mock.Verify(x => x.LoadSyncedModelsAsync(null, 12));
        }


        [Test]
        public async Task SHOULD_publish_locally_loaded_entities()
        {
            //Arrange
            var result = new List<SyncUpdate<TestModel>>();

            //Act
            Sut.Connect(_syncCommand).Subscribe(next =>
            {
                result.Add(next);
                if (result.Count == 3)
                {
                    _tcs.SetResult(result);
                }
            });
            await _tcs.Task;

            //Assert
            Assert.AreEqual(result[0].Current.Id, _localModels[0].Id);
            Assert.AreEqual(result[1].Current.Id, _localModels[1].Id);
            Assert.AreEqual(result[2].Current.Id, _localModels[2].Id);
        }

        [Test]
        public void SHOULD_trace_local_entity_info()
        {
            //Act
            Sut.Connect(_syncCommand).Subscribe();

            //Assert
            MockAnalyticsService.VerifyTrace("Initial batch of local models loaded");
            MockAnalyticsService.VerifyTraceProperty(nameof(ClientSyncStatus), _localSyncstatus);
            MockAnalyticsService.VerifyTraceProperty("Models published", 3);
        }

        
        [Test]
        public async Task SHOULD_sync_updates_from_server_using_loaded_sync_status()
        {
            //Arrange
            var result = new List<SyncUpdate<TestModel>>();

            //Act
            Sut.Connect(_syncCommand).Subscribe();

            //Assert
            MockSyncCommandHandler.Verify_HandleAsync_called_With(x => x.ModifiedBeforeTicks == _localSyncstatus.FirstModifiedAt);
            MockSyncCommandHandler.Verify_HandleAsync_called_With(x => x.ModifiedAfterTicks == _localSyncstatus.LastModifiedAt);
            MockSyncCommandHandler.Verify_HandleAsync_called_With(x => x.BatchSize == 12);
        }

        [Test]
        public async Task SHOULD_publish_updates_from_server_and_trace()
        {
            //Arrange
            var result = new List<SyncUpdate<TestModel>>();

            //Act
            Sut.Connect(_syncCommand).Subscribe(next =>
            {
                result.Add(next);
                if (result.Count == 6)
                {
                    _tcs.SetResult(result);
                }
            });
            await _tcs.Task;

            //Assert
            Assert.AreEqual(result[3].Current.Id, _serverModels[0].Id);
            Assert.AreEqual(result[4].Current.Id, _serverModels[1].Id);
            Assert.AreEqual(result[5].Current.Id, _serverModels[2].Id);
            MockAnalyticsService.VerifyTrace("Sync result received from server");
            MockAnalyticsService.VerifyTraceProperty(nameof(SyncResult<TestModel>.TotalEntityCount), _syncResult.TotalEntityCount);
            MockAnalyticsService.VerifyTraceProperty(nameof(SyncResult<TestModel>.ModifiedEntityCount), _syncResult.ModifiedEntityCount);
            MockAnalyticsService.VerifyTraceProperty(nameof(SyncResult<TestModel>.Entities), _syncResult.Entities.Count);
        }
    }
}