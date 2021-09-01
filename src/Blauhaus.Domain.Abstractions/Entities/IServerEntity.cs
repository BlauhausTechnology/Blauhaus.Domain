using System;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IServerEntity<out TId> : IEntity<TId>
    {
        DateTime CreatedAt { get; }
        DateTime ModifiedAt { get; }
    }
}