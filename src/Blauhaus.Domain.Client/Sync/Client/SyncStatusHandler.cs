using Blauhaus.Common.Utils.NotifyPropertyChanged;

namespace Blauhaus.Domain.Client.Sync.Client
{
    public class SyncStatusHandler : BaseBindableObject, ISyncStatusHandler
    {
        private long? _allLocalEntities;
        private long? _syncedLocalEntities;
        private long? _allServerEntities;
        private long? _newlyDownloadedEntities;
        private bool _isConnected = true;
        private string _statusMessage = string.Empty;
        private long? _publishedEntities;
        private SyncClientState _state;
        private long? _entitiesToDownload;

        public long? AllLocalEntities
        {
            get => _allLocalEntities;
            set => SetProperty(ref _allLocalEntities, value);
        }

        public long? SyncedLocalEntities
        {
            get => _syncedLocalEntities;
            set => SetProperty(ref _syncedLocalEntities, value);
        }

        public long? AllServerEntities
        {
            get => _allServerEntities;
            set => SetProperty(ref _allServerEntities, value);
        }
        
        public long? NewlyDownloadedEntities
        {
            get => _newlyDownloadedEntities;
            set => SetProperty(ref _newlyDownloadedEntities, value);
        }

        public long? PublishedEntities 
        {
            get => _publishedEntities;
            set => SetProperty(ref _publishedEntities, value);
        }

        public long? TotalEntitiesToDownload
        {
            get => _entitiesToDownload;
            set => SetProperty(ref _entitiesToDownload, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public SyncClientState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }
    }
}