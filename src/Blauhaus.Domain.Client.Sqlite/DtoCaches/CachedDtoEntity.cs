using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Newtonsoft.Json;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.DtoCaches
{
    public class CachedDtoEntity<TEntity, TDto, TId> : ClientEntity<TId>, ICachedDtoEntity<TEntity, TDto, TId>
        where TEntity : CachedDtoEntity<TEntity, TDto, TId>
        where TDto : ClientEntity<TId>, new()
    {
 
        [Indexed]
        public SyncState SyncState { get; set; }

        public string SerializedDto { get; set; } = string.Empty;

        
        public virtual TEntity FromDto(TDto dto)
        {
            Id = dto.Id;
            EntityState = dto.EntityState;
            ModifiedAtTicks = dto.ModifiedAtTicks;
            SerializedDto = JsonConvert.SerializeObject(dto);
            SyncState = SyncState.InSync;
            return (TEntity)this;
        }

        protected virtual void PopulateAdditionalProperties(TDto dto)
        {
        }

        public TDto ToDto()
        {
            var dto = JsonConvert.DeserializeObject<TDto>(SerializedDto);
            if (dto == null)
            {
                throw new InvalidOperationException($"Failed to deserialize {typeof(TDto).Name} from {typeof(TEntity).Name}");
            }

            return dto;
        }
         
    }
}