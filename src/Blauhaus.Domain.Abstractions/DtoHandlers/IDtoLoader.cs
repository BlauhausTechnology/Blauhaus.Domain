using Blauhaus.Domain.Abstractions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Abstractions.DtoHandlers
{
    public interface IDtoLoader<TDto, TId> where TDto : IClientEntity<TId> 
    {
        Task<TDto> GetDtoAsync();
    }

    public interface IDtoLoader<TDto> where TDto : IClientEntity
    {
        Task<TDto> GetDtoAsync();
    }

    
    public interface IDtosLoader<TDto> where TDto : IClientEntity
    {
        Task<List<TDto>> GetDtosAsync();
    }
}