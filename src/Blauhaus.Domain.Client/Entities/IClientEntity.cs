using System;
using Blauhaus.Domain.Client.CommandHandlers.Entities;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Entities
{
    public interface IClientEntity : IEntity
    {
        long ModifiedAtTicks { get; }
    }
}