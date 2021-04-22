using System;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.Entities
{
    public abstract class BaseServerUserEntity : BaseServerEntity, IHasUserId
    {
        protected internal BaseServerUserEntity()
        {
        }
         
        protected BaseServerUserEntity(DateTime createdAt, Guid id, EntityState entityState, Guid userId)
            : base(createdAt, id, entityState)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}