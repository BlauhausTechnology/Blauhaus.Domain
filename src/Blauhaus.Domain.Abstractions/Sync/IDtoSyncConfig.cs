namespace Blauhaus.Domain.Abstractions.Sync
{
    public interface IDtoSyncConfig
    {
        int GetSyncBatchSize(string dtoName);
    }
}