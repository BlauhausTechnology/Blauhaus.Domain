using System;
using Blauhaus.Common.Utils.Contracts;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.EFCore.Entities
{
    public class BaseServerUserEntity : BaseServerEntity, IHasUserId<Guid>
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