using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.EFCore.Entities
{ 
    public class BaseServerEntity : IServerEntity
    {
        protected internal BaseServerEntity()
        {
        }
         
        protected BaseServerEntity(DateTime createdAt, Guid id, EntityState entityState)
        {
            Id = id;
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
            EntityState = EntityState.Deleted;
            Modify(now);
        }
    }
}