using Blauhaus.Domain.Client.Sync.Model;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncModels
{
    public class SyncModelMockBuilder<TModel> : BaseSyncModelMockBuilder<SyncModelMockBuilder<TModel>, ISyncModel<TModel>, TModel>
        where TModel : IClientEntity 
    {
    }
}