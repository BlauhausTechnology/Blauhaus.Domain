namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface ICachedDtoEntity<out TEntity, TDto, out TId> : ISyncClientEntity<TId>
        where TDto : class, IClientEntity<TId>
        where TEntity : ICachedDtoEntity<TEntity, TDto, TId>
    {
        string SerializedDto { get; set; }

        TEntity FromDto(TDto dto);
        TDto ToDto();
    }
}