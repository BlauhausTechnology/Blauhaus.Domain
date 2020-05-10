namespace Blauhaus.Domain.Client.Sync
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