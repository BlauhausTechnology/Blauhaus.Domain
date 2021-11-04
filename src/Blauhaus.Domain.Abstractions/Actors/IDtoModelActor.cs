using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.Abstractions.Actors
{
    public interface IDtoModelActor<TModel, TDto, TId> : IModelActor<TModel, TId>, IDtoOwner<TDto>
        where TModel : IHasId<TId>
    {
        
    }
}