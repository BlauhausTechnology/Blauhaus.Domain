namespace Blauhaus.Domain.Client.Sync.Client
{
    public enum SyncClientState
    {
        NotStarted,
        Starting,
        LoadingLocal,
        DownloadingNew,
        DownloadingOld,
        Cancelled,
        Completed,
        Error
    }
}