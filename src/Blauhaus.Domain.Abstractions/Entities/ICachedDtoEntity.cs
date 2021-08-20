namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface ICachedDtoEntity<out TId> : ISyncClientEntity<TId>
    {
        string SerializedDto { get; set; }
    }
}