using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Newtonsoft.Json;

namespace Blauhaus.Domain.Client.Sqlite.DtoCaches
{
    public class CachedDtoConverter<TDto, TId, TCachedDtoEntity> : ICachedDtoConverter<TDto, TId, TCachedDtoEntity>
        where TDto : ClientEntity<TId>, new()
        where TCachedDtoEntity : CachedDtoEntity<TCachedDtoEntity, TDto, TId>, new()
    {
        public TDto ConvertToDto(TCachedDtoEntity entity)
        {
            var dto = JsonConvert.DeserializeObject<TDto>(entity.SerializedDto);
            if (dto == null)
            {
                throw new InvalidOperationException("Failed to convert entity to dto");
            }

            return dto;
        }

        public TCachedDtoEntity ConvertToEntity(TDto dto)
        {
            var serializedObject = JsonConvert.SerializeObject(dto);
            var entity = new TCachedDtoEntity
            {
                SerializedDto = serializedObject,
                EntityState = dto.EntityState,
                ModifiedAtTicks = dto.ModifiedAtTicks,
                Id = dto.Id
            };
            return PopulateEntity(entity, dto);
        }

        protected virtual TCachedDtoEntity PopulateEntity(TCachedDtoEntity cachedDtoEntity, TDto dto)
        {
            return cachedDtoEntity;
        }
    }
}