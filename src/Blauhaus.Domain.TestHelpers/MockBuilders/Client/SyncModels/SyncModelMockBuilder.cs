using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sync.Model;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncModels
{
    public class SyncModelMockBuilder<TModel> : BaseSyncModelMockBuilder<SyncModelMockBuilder<TModel>, ISyncModel<TModel>, TModel>
        where TModel : IClientEntity 
    {
    }
}