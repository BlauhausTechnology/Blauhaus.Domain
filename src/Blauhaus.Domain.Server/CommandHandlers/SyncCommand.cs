namespace Blauhaus.Domain.Server.CommandHandlers
{
    public class SyncCommand
    {
        public long? ModifiedAfter { get; set; }
        public long? ModifiedBefore { get; set; }
        public int BatchSize { get; set; } = 100;
    }
}