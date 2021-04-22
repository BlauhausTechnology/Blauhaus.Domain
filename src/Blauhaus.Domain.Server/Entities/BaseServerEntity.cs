﻿using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.Entities
{ 
    public abstract class BaseServerEntity : IServerEntity
    {
        protected BaseServerEntity()
        {
        }
         
        protected BaseServerEntity(DateTime createdAt, Guid id = default, EntityState entityState = EntityState.Active)
        {
            Id = id == default ? Guid.NewGuid() : id;
            EntityState = entityState;
            CreatedAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
            ModifiedAt = createdAt;
        }

        public Guid Id { get; private set; }
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