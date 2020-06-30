using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public interface IListItemUpdater<TModel, TViewElment>
        where TModel : IClientEntity
        where TViewElment : ListItem
    {
        TViewElment Update(TModel model, TViewElment element);
    }
}