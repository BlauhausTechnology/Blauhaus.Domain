namespace Blauhaus.Domain.Common.Entities
{
    public interface IClientEntity : IEntity
    {
        long ModifiedAtTicks { get; }
    }
}