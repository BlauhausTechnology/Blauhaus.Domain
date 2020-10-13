namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IClientEntity : IEntity
    {
        long ModifiedAtTicks { get; }
    }
}