using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncCollections
{

    public class BaseSyncCollectionMockBuilder<TBuilder, TModel, TListItem, TSyncCommand> : BaseMockBuilder<TBuilder, ISyncCollection<TModel, TListItem, TSyncCommand>> 
        where TBuilder : BaseSyncCollectionMockBuilder<TBuilder, TModel, TListItem, TSyncCommand>
        where TSyncCommand : SyncCommand, new()
        where TListItem : IListItem<TModel>
        where TModel : IClientEntity
    {
        public BaseSyncCollectionMockBuilder()
        {
            With(x => x.SyncCommand, new TSyncCommand());
            With(x => x.SyncRequirement, ClientSyncRequirement.All);
            With(x => x.SyncStatusHandler, new SyncStatusHandlerMockBuilder().Object);
        }
    }
}