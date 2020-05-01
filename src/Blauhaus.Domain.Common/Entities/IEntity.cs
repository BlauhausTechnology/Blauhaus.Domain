using System;

namespace Blauhaus.Domain.Common.Entities
{
    public interface IEntity
    {
        Guid Id { get; }
        EntityState EntityState { get; }
        long ModifiedAtTicks { get; }
    }
}