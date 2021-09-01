using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;

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
        where TModel : IClientEntity<Guid>
    {
        public ObservableCollection<TListItem> ListItems { get; }
        public TSyncCommand SyncCommand { get; }

    }
}