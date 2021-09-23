using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Sync.Old.Collection;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync.Old.SyncClients;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync.Old.SyncCollections
{

    public class BaseSyncCollectionMockBuilder<TBuilder, TModel, TListItem, TSyncCommand> : BaseMockBuilder<TBuilder, ISyncCollection<TModel, TListItem, TSyncCommand>> 
        where TBuilder : BaseSyncCollectionMockBuilder<TBuilder, TModel, TListItem, TSyncCommand>
        where TSyncCommand : SyncCommand, new()
        where TListItem : IListItem<TModel>
        where TModel : IClientEntity<Guid>
    {
        public BaseSyncCollectionMockBuilder()
        {
            With(x => x.SyncCommand, new TSyncCommand());
            With(x => x.SyncRequirement, ClientSyncRequirement.All);
            With(x => x.SyncStatusHandler, new SyncStatusHandlerMockBuilder().Object);
        }
    }
}