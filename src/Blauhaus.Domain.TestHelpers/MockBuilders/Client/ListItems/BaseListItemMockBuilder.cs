using System;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ListItems
{
    public class BaseListItemMockBuilder<TBuilder, TListItem, TModel> : BaseMockBuilder<TBuilder, TListItem>
        where TListItem : class, IListItem<TModel> 
        where TModel : IClientEntity
        where TBuilder : BaseListItemMockBuilder<TBuilder, TListItem, TModel>
    {

        public TBuilder Where_Update_throws(Exception e)
        {
            Mock.Setup(x => x.UpdateFromModel(It.IsAny<TModel>()))
                .Throws(e);
            return (TBuilder) this;
        }
    }
}