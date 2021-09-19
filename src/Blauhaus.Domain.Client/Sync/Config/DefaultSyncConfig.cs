using Blauhaus.Domain.Abstractions.Sync;

namespace Blauhaus.Domain.Client.Sync.Config
{
    public class DefaultSyncConfig : IDtoSyncConfig
    {
        public int GetSyncBatchSize(string dtoName)
        {
            return 100;
        }
    }
}