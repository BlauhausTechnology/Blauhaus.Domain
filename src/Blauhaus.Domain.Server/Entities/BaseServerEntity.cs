using System;
using System.Security.Cryptography;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.Entities
{

    public abstract class BaseServerEntity : BaseServerEntity<Guid>
    {
        protected BaseServerEntity()
        {
        }
         
        protected BaseServerEntity(DateTime createdAt, Guid id, EntityState entityState = EntityState.Active) 
            : base(createdAt, id, entityState)
        {
        }

        protected override Guid GenerateId()
        {
            return Guid.NewGuid();
        }
    }


    public abstract class BaseServerEntity<TId> : IServerEntity<TId>
    {
        protected BaseServerEntity()
        {
        }
         
        protected BaseServerEntity(DateTime createdAt, TId id, EntityState entityState = EntityState.Active)
        {
            Id = id;
            EntityState = entityState;
            CreatedAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
            ModifiedAt = createdAt;
        }

        protected abstract TId GenerateId();

        public TId Id { get; private set; }
        public EntityState EntityState { get; private set; }
        public DateTime CreatedAt { get; private set;}
        public DateTime ModifiedAt { get; private set;}

        public void Modify(DateTime now)
        {
            ModifiedAt = DateTime.SpecifyKind(now, DateTimeKind.Utc);
        }
        
        public void Delete(DateTime now)
        {
            if (EntityState != EntityState.Deleted)
            {
                EntityState = EntityState.Deleted;
                Modify(now);
            }
        }
        
        public void Archive(DateTime now)
        {
            if (EntityState != EntityState.Archived)
            {
                EntityState = EntityState.Archived;
                Modify(now);
            }
        }
        
        public void Activate(DateTime now)
        {
            if (EntityState != EntityState.Active)
            {
                EntityState = EntityState.Active;
                Modify(now);
            }
        }
    }
}