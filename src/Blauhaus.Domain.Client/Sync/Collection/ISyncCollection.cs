using System.Collections.ObjectModel;
using System.ComponentModel;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{

    public interface ISyncCollection : INotifyPropertyChanged
    {
        
        public ClientSyncRequirement SyncRequirement { get; set; }
        public ISyncStatusHandler SyncStatusHandler { get; }

        void Initialize();
        void LoadNewFromServer();
        void LoadNewFromClient();
        void ReloadFromClient();
    }

    public interface ISyncCollection<TModel, TListItem, TSyncCommand> : ISyncCollection
        where TListItem : IListItem<TModel>
        where TSyncCommand : SyncCommand, new()
        where TModel : IClientEntity
    {
        public ObservableCollection<TListItem> ListItems { get; }
        public TSyncCommand SyncCommand { get; }

    }
}