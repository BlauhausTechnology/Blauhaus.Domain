using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.Abstractions.Actors
{
    public interface IDtoActor<TDto, TId> : IActor<TId>, IDtoOwner<TDto> 
    {
        
    }
}