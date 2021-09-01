using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.Service;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncServiceTests
{
    public class SyncTests : BaseDomainTest<SyncService<TestSyncCommand>>
    {
        private TaskCompletionSource<List<SyncUpdate>> _tcs;
        private List<SyncUpdate> _syncUpdates;
        private SyncStatusHandlerMockBuilder _syncHandler;
        private SyncStatusHandlerMockBuilder _syncHandlerToo;
        
        protected SyncClientMockBuilder<TestModel, TestSyncCommand> MockSyncClient => Mocks.AddMockSyncClient<TestModel, TestSyncCommand>().Invoke();
        protected SyncClientMockBuilder<TestModelToo, TestSyncCommand> MockSyncClientToo => Mocks.AddMockSyncClient<TestModelToo, TestSyncCommand>().Invoke();
        protected MockBuilder<ISyncClientFactory<TestSyncCommand>> MockSyncClientFactory => AddMock<ISyncClientFactory<TestSyncCommand>>().Invoke();

        public override void Setup()
        {
            base.Setup();

            AddService(x => MockSyncClient.Object);
            AddService(x => MockSyncClientToo.Object);
            AddService(x => MockSyncClientFactory.Object);

            MockSyncClient.Where_Connect_returns(new List<TestModel>());
            MockSyncClientToo.Where_Connect_returns(new List<TestModelToo>());
            MockSyncClientFactory.With(x => x.SyncConnections, new List<Func<TestSyncCommand, ClientSyncRequirement, ISyncStatusHandler, IObservable<object>>>
            {
                (command, req, handler) => MockSyncClient.Object.Connect(command, req, handler),
                (command, req, handler) => MockSyncClientToo.Object.Connect(command, req, handler),
            });

            _syncHandler = new SyncStatusHandlerMockBuilder();
            _syncHandlerToo = new SyncStatusHandlerMockBuilder();
            _tcs = new TaskCompletionSource<List<SyncUpdate>>();
            _syncUpdates = new List<SyncUpdate>();

            MockServiceLocator.Where_Resolve_returns_sequence(new List<ISyncStatusHandler>{ _syncHandler.Object, _syncHandlerToo.Object});
        }

        [Test]
        public void SHOULD_invoke_SyncClients_using_syncStatusHandlers_from_factory()
        {
            //Act
            Sut.Sync().Subscribe();

            //Assert
            MockSyncClient.Mock.Verify(x => x.Connect(It.IsAny<TestSyncCommand>(), It.Is<ClientSyncRequirement>(y => 
                y.SyncAll == true), _syncHandler.Object));
            MockSyncClientToo.Mock.Verify(x => x.Connect(It.IsAny<TestSyncCommand>(), It.Is<ClientSyncRequirement>(y => 
                y.SyncAll == true), _syncHandlerToo.Object));
        }

        [Test]
        public async Task SHOULD_publish_first_update_with_EntityType_count()
        {
            //Arrange
            
            //Act
            Sut.Sync().Subscribe(next =>
            {
                _syncUpdates.Add(next);
                _tcs.SetResult(_syncUpdates);
            });
            await _tcs.Task;

            //Assert
            Assert.That(_syncUpdates[0].EntityTypesToSync, Is.EqualTo(2));
            Assert.That(_syncUpdates[0].EntityTypesSynced, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntityTypeSyncProgress, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntitiesToSync, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntitiesSynced, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntitySyncProgress, Is.EqualTo(0));

        }
        
        [Test]
        public async Task WHEN_SyncStatusUpdateHandlers_are_set_to_completed_SHOULD_update_EntityTypesSynced()
        {
            //Arrange
            Sut.Sync().Subscribe(next =>
            {
                _syncUpdates.Add(next);
                if (_syncUpdates.Count == 3)
                {
                    _tcs.SetResult(_syncUpdates);
                }
            });
            
            //Act
            _syncHandler.With(x => x.State, SyncClientState.Completed);
            _syncHandler.RaisePropertyChanged();
            _syncHandlerToo.With(x => x.State, SyncClientState.Completed);
            _syncHandlerToo.RaisePropertyChanged();
            await _tcs.Task;

            //Assert
            Assert.That(_syncUpdates[0].EntityTypesToSync, Is.EqualTo(2));
            Assert.That(_syncUpdates[0].EntityTypesSynced, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntityTypeSyncProgress, Is.EqualTo(0)); 
            Assert.That(_syncUpdates[1].EntityTypesToSync, Is.EqualTo(2));
            Assert.That(_syncUpdates[1].EntityTypesSynced, Is.EqualTo(1));
            Assert.That(_syncUpdates[1].EntityTypeSyncProgress, Is.EqualTo(0.5f)); 
            Assert.That(_syncUpdates[2].EntityTypesToSync, Is.EqualTo(2));
            Assert.That(_syncUpdates[2].EntityTypesSynced, Is.EqualTo(2));
            Assert.That(_syncUpdates[2].EntityTypeSyncProgress, Is.EqualTo(1f)); 
        }

        [Test]
        public async Task WHEN_All_EntityTypes_are_synced_SHOULD_complete()
        {
            //Arrange
            List<bool> isCompleteUpdates = new List<bool>();
            Sut.Sync().Subscribe(next =>
            {
                isCompleteUpdates.Add(next.IsCompleted);
            }, () =>
            {
                _tcs.SetResult(_syncUpdates);
            });
            
            //Act
            _syncHandler.With(x => x.State, SyncClientState.Completed);
            _syncHandler.RaisePropertyChanged(); 
            _syncHandlerToo.With(x => x.State, SyncClientState.Completed);
            _syncHandlerToo.RaisePropertyChanged();
            await _tcs.Task;

            //Assert
            Assert.That(isCompleteUpdates[0], Is.False);
            Assert.That(isCompleteUpdates[1], Is.False);
            Assert.That(isCompleteUpdates[2], Is.True); 
        }

        [Test]
        public async Task WHEN_SyncClient_returns_error_SHOULD_OnError()
        {
            //Arrange
            MockSyncClient.Where_Connect_returns_exception(new Exception("oops"));
            Exception? exception = null;
            
            //Act
            Sut.Sync().Subscribe(next =>
            {
            }, (e) =>
            {
                exception = e;
                _tcs.SetResult(_syncUpdates);
            });
            await _tcs.Task;

            //Assert
            Assert.That(exception != null);
            Assert.That(exception.Message, Is.EqualTo("oops"));
        }

        [Test]
        public async Task SHOULD_log_start_and_finish_with_duration()
        {
            //Arrange
            
            Sut.Sync().Subscribe(next =>
            {
            }, () =>
            {
                _tcs.SetResult(_syncUpdates);
            });
            
            //Act
            _syncHandler.With(x => x.State, SyncClientState.Completed);
            _syncHandler.With(x => x.NewlyDownloadedEntities, 50);
            _syncHandler.RaisePropertyChanged(); 
            _syncHandlerToo.With(x => x.State, SyncClientState.Completed);
            _syncHandlerToo.With(x => x.NewlyDownloadedEntities, 100);
            _syncHandlerToo.RaisePropertyChanged();
            await _tcs.Task;

            //Assert 
            MockAnalyticsService.VerifyTrace("Sync started for 2 entity types", LogSeverity.Information);
            MockAnalyticsService.VerifyTrace("Sync completed for 2 entity types. 150 entities synced", LogSeverity.Information);
        }
        [Test]
        public async Task WHEN_SyncStatusUpdateHandlers_update_entities_SHOULD_update()
        {
            //Arrange
            Sut.Sync().Subscribe(next =>
            {
                _syncUpdates.Add(next);
                if (_syncUpdates.Count == 3)
                {
                    _tcs.SetResult(_syncUpdates);
                }
            });
            
            //Act
            _syncHandler.With(x => x.TotalEntitiesToDownload, 100);
            _syncHandler.With(x => x.NewlyDownloadedEntities, 10);
            _syncHandler.RaisePropertyChanged();
            _syncHandler.With(x => x.TotalEntitiesToDownload, 100);
            _syncHandler.With(x => x.NewlyDownloadedEntities, 20);
            _syncHandler.RaisePropertyChanged();
            await _tcs.Task;

            //Assert
            Assert.That(_syncUpdates[0].EntitiesToSync, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntitiesSynced, Is.EqualTo(0));
            Assert.That(_syncUpdates[0].EntitySyncProgress, Is.EqualTo(0)); 
            Assert.That(_syncUpdates[1].EntitiesToSync, Is.EqualTo(100));
            Assert.That(_syncUpdates[1].EntitiesSynced, Is.EqualTo(10));
            Assert.That(_syncUpdates[1].EntitySyncProgress, Is.EqualTo(0.1f)); 
            Assert.That(_syncUpdates[2].EntitiesToSync, Is.EqualTo(100));
            Assert.That(_syncUpdates[2].EntitiesSynced, Is.EqualTo(20));
            Assert.That(_syncUpdates[2].EntitySyncProgress, Is.EqualTo(0.2f)); 
        }

    }
}