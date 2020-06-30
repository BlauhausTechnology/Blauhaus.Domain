using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.Entities
{

    public class SyncClientEntityMockBuilder : BaseSyncClientEntityMockBuilder<SyncClientEntityMockBuilder, ISyncClientEntity>
    {
    }

    public class BaseSyncClientEntityMockBuilder<TBuilder, TMock> : BaseClientEntityMockBuilder<TBuilder, TMock> 
        where TBuilder : BaseSyncClientEntityMockBuilder<TBuilder, TMock>
        where TMock : class, ISyncClientEntity
    {
        protected BaseSyncClientEntityMockBuilder()
        {
            With(x => x.SyncState, SyncState.InSync);
        }
    
    }
}