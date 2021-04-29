namespace Blauhaus.Domain.Abstractions.Commands
{
    public class GetCommand
    {
        public long? ModifiedAfter { get; set; }
        public long? ModifiedBefore { get; set; }
        public int BatchSize { get; set; } = 100;
    }
}