namespace Blauhaus.Domain.Abstractions.Sync
{
    public interface ISyncStatus
    {
        public int DownloadedDtoCount { get; }
        public int TotalDtoCount { get; }
        public float Progress { get; }
    }
}