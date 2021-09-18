using System;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IServerEntity : IEntity<Guid>
    {
        DateTime CreatedAt { get; }
        DateTime ModifiedAt { get; }
    }
}