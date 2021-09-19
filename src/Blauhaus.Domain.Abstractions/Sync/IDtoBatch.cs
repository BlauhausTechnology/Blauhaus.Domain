namespace Blauhaus.Domain.Abstractions.Sync
{
    public interface IDtoBatch<TDto> : IDtoBatch
    {
    }

    public interface IDtoBatch
    {
        int CurrentDtoCount { get; } 
        int RemainingDtoCount { get; } 
        long BatchLastModifiedTicks { get; } 
    }
}