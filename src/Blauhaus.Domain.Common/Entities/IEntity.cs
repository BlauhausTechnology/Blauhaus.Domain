using System;
using Blauhaus.Domain.Client.CommandHandlers.Entities;

namespace Blauhaus.Domain.Common.Entities
{
    public interface IEntity
    {
        
        Guid Id { get; }
        EntityState EntityState { get; }
    }
}