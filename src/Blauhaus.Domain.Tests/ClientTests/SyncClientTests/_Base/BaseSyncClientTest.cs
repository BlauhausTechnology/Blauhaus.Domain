using System.Collections.Generic;
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

namespace Blauhaus.Domain.Tests.ClientTests.SyncClientTests._Base
{
    public class BaseSyncClientTest : BaseDomainTest<SyncClient<TestModel, TestModelDto, TestSyncCommand>>
    {
        
        protected TestSyncCommand SyncCommand; 
        protected ClientSyncRequirement ClientSyncRequirement;
        protected List<string> StatusMessages;
        protected List<SyncClientState> StateUpdates;


        protected MockBuilder<ISyncStatusHandler> MockSyncStatusHandler => AddMock<ISyncStatusHandler>().Invoke();
        protected ConnectivityServiceMockBuilder MockConnectivityService => AddMock<ConnectivityServiceMockBuilder, IConnectivityService>().Invoke();
        protected MockBuilder<ITimeService> MockTimeService => AddMock<ITimeService>().Invoke();

        protected SyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand> MockSyncClientRepository 
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
            
            MockSyncStatusHandler.Mock.SetupAllProperties();
            StatusMessages = new List<string>();
            MockSyncStatusHandler.Mock.SetupSet(x => x.StatusMessage)
                .Callback(message => StatusMessages.Add(message));

            StateUpdates = new List<SyncClientState>();
            MockSyncStatusHandler.Mock.SetupSet(x => x.State)
                .Callback(state => StateUpdates.Add(state));
            
            MockSyncCommandHandler.Where_HandleAsync_returns(new SyncResult<TestModel>{EntityBatch = new List<TestModel>()});
            MockSyncClientRepository.Where_GetSyncStatusAsync_returns(new ClientSyncStatus());
            MockSyncClientRepository.Where_LoadModelsAsync_returns(new List<TestModel>());

            AddService(MockSyncClientRepository.Object);
            AddService(MockSyncCommandHandler.Object);
            AddService(MockTimeService.Object);
            AddService(MockConnectivityService.Object);
        }
    }
}