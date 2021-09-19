using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.TestHelpers.MockBuilders;
using System;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Sync.Old.Collection;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncCollections
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