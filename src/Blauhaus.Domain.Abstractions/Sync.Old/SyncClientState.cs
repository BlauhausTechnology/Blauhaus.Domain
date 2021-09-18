namespace Blauhaus.Domain.Abstractions.Sync.Old
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