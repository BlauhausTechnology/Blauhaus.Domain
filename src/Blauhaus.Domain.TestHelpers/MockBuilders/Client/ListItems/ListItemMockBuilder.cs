using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ListItems
{
    public class ListItemMockBuilder<TListItem, TModel> : BaseListItemMockBuilder<ListItemMockBuilder<TListItem, TModel>, TListItem, TModel>  
        where TListItem : class, IListItem<TModel> 
        where TModel : IClientEntity
    {
        
    }
}