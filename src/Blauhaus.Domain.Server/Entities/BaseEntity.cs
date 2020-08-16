using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.Entities
{
    public abstract class BaseEntity : IServerEntity
    {
        protected BaseEntity()
        {
        }

        protected BaseEntity(DateTime now, Guid id = default)
        {

            if (now.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("ModifiedAt must be UTC");
            }

            CreatedAt = now;
            ModifiedAt = now;
            Id = id == default ? Guid.NewGuid() : id;
            EntityState = EntityState.Active;
        }

        public Guid Id { get; private set; }
        public EntityState EntityState { get; protected set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime ModifiedAt { get; private set; }

        public void UpdateModifiedAt(DateTime now)
        {
            if (now.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("ModifiedAt must be UTC");
            }
            ModifiedAt = now;
        } 
    }
}