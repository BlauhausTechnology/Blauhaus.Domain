namespace Blauhaus.Domain.Abstractions.Sync
{
    public class DtoSyncCommand
    {
        public DtoSyncCommand(long? modifiedAfterTicks, int? maxResults)
        {
            ModifiedAfterTicks = modifiedAfterTicks;
            MaxResults = maxResults;
        }

        public long? ModifiedAfterTicks { get; }
        public int? MaxResults { get; }
    }
}