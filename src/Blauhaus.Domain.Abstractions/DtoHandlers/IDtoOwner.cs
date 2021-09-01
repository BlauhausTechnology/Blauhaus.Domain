using Blauhaus.Domain.Abstractions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Abstractions.DtoHandlers
{
    public interface IDtoOwner<TDto, TId> where TDto : IClientEntity<TId> 
    {
        Task<TDto> GetDtoAsync();
    }

    public interface IDtoOwner<TDto> where TDto : IClientEntity
    {
        Task<TDto> GetDtoAsync();
    }
     
}