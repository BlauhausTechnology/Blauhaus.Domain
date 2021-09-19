namespace Blauhaus.Domain.Abstractions.Sync
{
    //just using TDto as a marker for ioc
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