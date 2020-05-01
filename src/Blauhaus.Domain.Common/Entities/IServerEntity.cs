using System;

namespace Blauhaus.Domain.Common.Entities
{
    public interface IServerEntity : IEntity
    {
        DateTime CreatedAt { get; }
        DateTime ModifiedAt { get; }
    }
}