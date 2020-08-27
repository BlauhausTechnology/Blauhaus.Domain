namespace Blauhaus.Domain.Abstractions.Sync
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