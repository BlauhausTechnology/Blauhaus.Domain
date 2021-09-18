using Blauhaus.Common.Utils.Attributes;
using Blauhaus.Common.Utils.NotifyPropertyChanged;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Client.Sync.Old.Client
{
    [Preserve]
    public class SyncStatusHandler : BaseBindableObject, ISyncStatusHandler
    {
 
        public long? AllLocalEntities
        {
            get => GetProperty<long?>();
            set => SetProperty(value);
        }

        public long? SyncedLocalEntities
        {
            get => GetProperty<long?>();
            set => SetProperty(value);
        }

        public long? AllServerEntities
        {
            get => GetProperty<long?>();
            set => SetProperty(value);
        }
        
        public long? NewlyDownloadedEntities
        {
            get => GetProperty<long?>();
            set => SetProperty(value);
        }

        public long? PublishedEntities 
        {
            get => GetProperty<long?>();
            set => SetProperty(value);
        }

        public long? TotalEntitiesToDownload
        {
            get => GetProperty<long?>();
            set => SetProperty(value);
        }

        public bool IsConnected
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string StatusMessage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public SyncClientState State
        {
            get => GetProperty<SyncClientState>();
            set => SetProperty(value);
        }
    }
}