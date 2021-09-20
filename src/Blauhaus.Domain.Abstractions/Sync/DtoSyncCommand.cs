namespace Blauhaus.Domain.Abstractions.Sync
{
    public class DtoSyncCommand
    {
        public DtoSyncCommand(long? modifiedAfterTicks)
        {
            ModifiedAfterTicks = modifiedAfterTicks;
        }

        public long? ModifiedAfterTicks { get; }
    }
}