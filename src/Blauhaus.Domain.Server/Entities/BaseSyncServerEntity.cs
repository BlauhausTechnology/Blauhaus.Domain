using System;
using System.Security.Cryptography;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Server.Entities
{
    public abstract class BaseSyncServerEntity : BaseSyncServerEntity<Guid>
    {
        protected BaseSyncServerEntity()
        {
        }
        protected BaseSyncServerEntity(
            DateTime createdAt, Guid id, EntityState entityState, string serializedDto)
            : base(createdAt, id, entityState, serializedDto)
        {
        }

        protected override Guid GenerateId()
        {
            return Guid.NewGuid();
        }
    }

    public abstract class BaseSyncServerEntity<TId> : BaseServerEntity<TId>, ISerializedDto
    {
        protected BaseSyncServerEntity()
        {
        }
         
        protected BaseSyncServerEntity(
            DateTime createdAt, TId id, EntityState entityState, string serializedDto)
                : base(createdAt, id, entityState)
        {
            SerializedDto = serializedDto;
        }

        public string SerializedDto { get; private set; } = string.Empty;
    }
}