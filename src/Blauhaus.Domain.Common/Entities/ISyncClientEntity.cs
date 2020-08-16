namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface ISyncClientEntity : IClientEntity
    {
        SyncState SyncState { get; set; }
    }
}