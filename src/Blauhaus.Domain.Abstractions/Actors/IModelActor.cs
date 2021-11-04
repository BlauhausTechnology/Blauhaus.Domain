using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.Abstractions.Actors
{
    public interface IModelActor<TModel, TId> : IActor<TId>, IAsyncPublisher<TModel>, IModelOwner<TModel>
        where TModel : IHasId<TId>
    {
    }
}