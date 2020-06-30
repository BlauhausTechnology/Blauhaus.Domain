using System.Collections.ObjectModel;
using System.ComponentModel;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public interface ISyncCollection<TListItem, TSyncCommand> : INotifyPropertyChanged
        where TListItem : ListItem, new()
        where TSyncCommand : SyncCommand, new()
    {
        public ClientSyncRequirement SyncRequirement { get; set; }
        public ObservableCollection<TListItem> ListItems { get; }
        public TSyncCommand SyncCommand { get; }
        public ISyncStatusHandler SyncStatusHandler { get; }

        void Initialize();
    }
}