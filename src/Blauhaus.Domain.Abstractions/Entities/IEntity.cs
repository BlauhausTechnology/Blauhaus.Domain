using System;
using Blauhaus.Common.Abstractions;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface IEntity<out TId> : IHasId<TId>
    {
        EntityState EntityState { get; }
    }
    
    public interface IEntity: IEntity<Guid>
    {
        
    }
    
}