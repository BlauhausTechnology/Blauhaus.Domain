﻿using System.Collections.Generic;
using Blauhaus.Common.Time.Service;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests._Base
{
    public class BaseSyncClientTest : BaseDomainTest<SyncClient<TestModel, TestModelDto, TestSyncCommand>>
    {
        
        protected TestSyncCommand SyncCommand; 
        protected ClientSyncRequirement ClientSyncRequirement;
        
        protected List<string> StatusMessages => MockSyncStatusHandler.StatusMessages;
        protected List<SyncClientState> StateUpdates => MockSyncStatusHandler.StateUpdates;


        protected SyncStatusHandlerMockBuilder MockSyncStatusHandler => AddMock<SyncStatusHandlerMockBuilder, ISyncStatusHandler>().Invoke();
        protected ConnectivityServiceMockBuilder MockConnectivityService => AddMock<ConnectivityServiceMockBuilder, IConnectivityService>().Invoke();
        protected MockBuilder<ITimeService> MockTimeService => AddMock<ITimeService>().Invoke();

        protected SyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand> MockBaseSyncClientRepository 
            => AddMock<SyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand>, ISyncClientRepository<TestModel, TestModelDto, TestSyncCommand>>().Invoke();

        protected CommandHandlerMockBuilder<SyncResult<TestModel>, TestSyncCommand> MockSyncCommandHandler 
            => AddMock<CommandHandlerMockBuilder<SyncResult<TestModel>, TestSyncCommand>, ICommandHandler<SyncResult<TestModel>, TestSyncCommand>>().Invoke();

        public override void Setup()
        {
            base.Setup();
            
            SyncCommand = new TestSyncCommand
            {
                BatchSize = 3,
                FavouriteFood = "Lasagne"
            }; 
            ClientSyncRequirement = ClientSyncRequirement.Batch;
             
            
            MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>{EntityBatch = new List<TestModel>()});
            MockBaseSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus());
            MockBaseSyncClientRepository.Where_LoadModelsAsync_returns(new List<TestModel>());

            AddService(MockBaseSyncClientRepository.Object);
            AddService(MockSyncCommandHandler.Object);
            AddService(MockTimeService.Object);
            AddService(MockConnectivityService.Object);
        }
    }
}