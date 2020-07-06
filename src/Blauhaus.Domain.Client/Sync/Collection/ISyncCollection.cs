using System.Collections.ObjectModel;
using System.ComponentModel;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public interface ISyncCollection<TModel, TListItem, TSyncCommand> : INotifyPropertyChanged
        where TListItem : IListItem<TModel>
        where TSyncCommand : SyncCommand, new()
        where TModel : IClientEntity
    {
        public ClientSyncRequirement SyncRequirement { get; set; }
        public ObservableCollection<TListItem> ListItems { get; }
        public TSyncCommand SyncCommand { get; }
        public ISyncStatusHandler SyncStatusHandler { get; }

        void Initialize();
        void Refresh();
    }
}