using System;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Server.Entities
{
    public interface IServerEntity : IEntity
    {
        DateTime CreatedAt { get; }
        DateTime ModifiedAt { get; }
    }
}