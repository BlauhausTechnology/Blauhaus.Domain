using System;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IEntity
    {
        Guid Id { get; }
        EntityState EntityState { get; }
    }
}