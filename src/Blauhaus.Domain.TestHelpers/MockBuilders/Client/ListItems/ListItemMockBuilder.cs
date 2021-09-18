using Blauhaus.Domain.Abstractions.Entities;
using System;
using Blauhaus.Domain.Client.Sync.Old.Collection;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ListItems
{
    public class ListItemMockBuilder<TListItem, TModel> : BaseListItemMockBuilder<ListItemMockBuilder<TListItem, TModel>, TListItem, TModel>  
        where TListItem : class, IListItem<TModel> 
        where TModel : IClientEntity<Guid>
    {
        
    }
}