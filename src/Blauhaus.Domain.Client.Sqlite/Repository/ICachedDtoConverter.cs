using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sqlite.Entities;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public interface ICachedDtoConverter<TDto, TId, TCachedDtoEntity>
        where TDto : class, IClientEntity<TId>
        where TCachedDtoEntity : ICachedDtoEntity<TCachedDtoEntity, TDto, TId>
    {
        TDto ConvertToDto(TCachedDtoEntity entity);
        TCachedDtoEntity ConvertToEntity(TDto dto);
    }
}