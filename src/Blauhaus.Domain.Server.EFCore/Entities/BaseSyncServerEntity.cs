using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Server.Entities;
using System;

namespace Blauhaus.Domain.Server.EFCore.Entities
{
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