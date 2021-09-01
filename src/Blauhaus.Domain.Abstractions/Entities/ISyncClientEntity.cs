namespace Blauhaus.Domain.Abstractions.Entities
{
    
    public interface ISyncClientEntity<out TId> : IClientEntity<TId>
    {
        SyncState SyncState { get; set; }
        string SerializedDto { get; set; }
    }
    
    public interface ISyncClientEntity : IClientEntity
    {
        SyncState SyncState { get; set; }
        string SerializedDto { get; set; }
    }
    
}