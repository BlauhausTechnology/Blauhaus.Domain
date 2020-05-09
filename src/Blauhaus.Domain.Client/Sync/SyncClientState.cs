namespace Blauhaus.Domain.Client.Sync
{
    public enum SyncClientState
    {
        NotStarted,
        LoadingLocal,
        DownloadingNew,
        DownloadingOld,
        Completed
    }
}