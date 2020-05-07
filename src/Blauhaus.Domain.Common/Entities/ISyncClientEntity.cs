namespace Blauhaus.Domain.Common.Entities
{
    public interface ISyncClientEntity : IClientEntity
    {
        SyncState SyncState { get; set; }
    }
}