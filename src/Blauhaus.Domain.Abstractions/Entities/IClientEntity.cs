using System;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IClientEntity<out TId> : IEntity<TId>
    {
        long ModifiedAtTicks { get; }
    }
    
    public interface IClientEntity : IEntity 
    {
        long ModifiedAtTicks { get; }
    }
}