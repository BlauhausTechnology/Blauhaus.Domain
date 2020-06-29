using System.Collections.Generic;
using System.ComponentModel;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients
{
    public class SyncStatusHandlerMockBuilder : BaseMockBuilder<SyncStatusHandlerMockBuilder, ISyncStatusHandler>
    {
        public SyncStatusHandlerMockBuilder()
        {
            Mock.SetupAllProperties();

            Mock.SetupSet(x => x.StatusMessage = It.IsAny<string>())
                .Callback<string>(message => StatusMessages.Add(message));

            Mock.SetupSet(x => x.State = It.IsAny<SyncClientState>())
                .Callback<SyncClientState>(state => StateUpdates.Add(state));
        }

        public List<SyncClientState> StateUpdates { get; } = new List<SyncClientState>();

        public List<string> StatusMessages { get; } = new List<string>();

        public void RaisePropertyChanged(string propertyName = "")
        {
            Mock.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs(propertyName));
        }
    }
}