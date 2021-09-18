using System.Threading.Tasks;

namespace Blauhaus.Domain.Abstractions.DtoHandlers
{ 
    public interface IDtoOwner<TDto> 
    {
        Task<TDto> GetDtoAsync();
    }
     
}