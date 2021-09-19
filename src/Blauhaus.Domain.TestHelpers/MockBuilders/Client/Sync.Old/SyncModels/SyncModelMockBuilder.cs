using Blauhaus.Domain.Abstractions.Entities;
using System;
using Blauhaus.Domain.Client.Sync.Old.Model;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncModels
{
    public class SyncModelMockBuilder<TModel> : BaseSyncModelMockBuilder<SyncModelMockBuilder<TModel>, ISyncModel<TModel>, TModel>
        where TModel : IClientEntity <Guid>
    {
    }
}