namespace Blauhaus.Domain.Client.Sync.DtoBatches
{
    public interface IDtoBatch
    {
        int CurrentDtoCount { get; } 
        int RemainingDtoCount { get; } 
        long BatchLastModifiedTicks { get; } 
    }
}