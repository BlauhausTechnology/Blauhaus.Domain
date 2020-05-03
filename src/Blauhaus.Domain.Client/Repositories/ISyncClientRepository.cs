using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Repositories
{
    public interface ISyncClientRepository<TModel, in TDto> : IClientRepository<TModel, TDto>
        where TModel : class, IClientEntity
    {
        Task<ClientSyncStatus> GetSyncStatusAsync();
        Task<IReadOnlyList<TModel>> LoadSyncedModelsAsync(long? modifiedBefore, int batchSize);
        Task<IReadOnlyList<TModel>> SaveSyncedDtosAsync(IReadOnlyList<TDto> dtos); 
    }
}