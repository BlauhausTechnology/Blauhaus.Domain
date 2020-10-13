using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ListItems
{
    public class BaseListItemMockBuilder<TBuilder, TListItem, TModel> : BaseMockBuilder<TBuilder, TListItem>
        where TListItem : class, IListItem<TModel> 
        where TModel : IClientEntity
        where TBuilder : BaseListItemMockBuilder<TBuilder, TListItem, TModel>
    {

        public BaseListItemMockBuilder()
        {
            Where_Update_returns(true);
        }

        public TBuilder Where_Update_throws(Exception e)
        {
            Mock.Setup(x => x.UpdateFromModel(It.IsAny<TModel>()))
                .Throws(e);
            return (TBuilder) this;
        }

        
        public TBuilder Where_Update_returns(bool result)
        {
            Mock.Setup(x => x.UpdateFromModel(It.IsAny<TModel>()))
                .Returns(result);
            return (TBuilder) this;
        }

        public TBuilder Where_Update_returns_sequence(params bool[] results)
        {
            var queue = new Queue<bool>(results);
            Mock.Setup(x => x.UpdateFromModel(It.IsAny<TModel>()))
                .Returns(queue.Dequeue);
            return (TBuilder) this;
        }
    }
}