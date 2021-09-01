using Blauhaus.Domain.Abstractions.Entities;
using System;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.Entities
{

    public class SyncClientEntityMockBuilder : BaseSyncClientEntityMockBuilder<SyncClientEntityMockBuilder, ISyncClientEntity<Guid>>
    {
    }

    public class BaseSyncClientEntityMockBuilder<TBuilder, TMock> : BaseClientEntityMockBuilder<TBuilder, TMock> 
        where TBuilder : BaseSyncClientEntityMockBuilder<TBuilder, TMock>
        where TMock : class, ISyncClientEntity<Guid>
    {
        protected BaseSyncClientEntityMockBuilder()
        {
            With(x => x.SyncState, SyncState.InSync);
        }
    
    }
}