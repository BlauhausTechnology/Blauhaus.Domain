using System;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IServerEntity : IEntity
    {
        DateTime CreatedAt { get; }
        DateTime ModifiedAt { get; }
    }
}