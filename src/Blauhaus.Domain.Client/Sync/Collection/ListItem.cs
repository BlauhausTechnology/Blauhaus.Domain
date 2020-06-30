using System;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public class ListItem : IClientEntity
    { 

        public Guid Id { get; set; }
        public bool IsVisible { get; set; } //todo
        public long ModifiedAtTicks { get; set; }
        public EntityState EntityState { get; } = EntityState.Active;

    }
}