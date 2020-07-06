using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncCollections
{
    public class SyncCollectionMockBuilder<TModel, TListItem> : BaseSyncCollectionMockBuilder<SyncCollectionMockBuilder<TModel, TListItem>, TModel, TListItem, SyncCommand>
        where TListItem : IListItem<TModel>
        where TModel : IClientEntity
    {
    }

    public class SyncCollectionMockBuilder<TModel, TListItem, TSyncCommand> : BaseSyncCollectionMockBuilder<SyncCollectionMockBuilder<TModel, TListItem, TSyncCommand>, TModel, TListItem, TSyncCommand>
        where TSyncCommand : SyncCommand, new()
        where TListItem : IListItem<TModel>
        where TModel : IClientEntity
    {
    }

    public class BaseSyncCollectionMockBuilder<TBuilder, TModel, TListItem, TSyncCommand> : BaseMockBuilder<TBuilder, ISyncCollection<TModel, TListItem, TSyncCommand>> 
        where TBuilder : BaseSyncCollectionMockBuilder<TBuilder, TModel, TListItem, TSyncCommand>
        where TSyncCommand : SyncCommand, new()
        where TListItem : IListItem<TModel>
        where TModel : IClientEntity
    {

    }
}