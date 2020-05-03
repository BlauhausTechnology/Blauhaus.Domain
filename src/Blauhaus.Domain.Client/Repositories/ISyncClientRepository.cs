using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Repositories
{

    public interface ISyncClientRepository<TModel, in TDto> : ISyncClientRepository<TModel, TDto, SyncCommand>
        where TModel : class, IClientEntity
    {

    }


    public interface ISyncClientRepository<TModel, in TDto, TSyncCommand> : IClientRepository<TModel, TDto>
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
    {
        Task<ClientSyncStatus> GetSyncStatusAsync();
        Task<IReadOnlyList<TModel>> LoadSyncedModelsAsync(TSyncCommand syncCommand);
        Task<IReadOnlyList<TModel>> SaveSyncedDtosAsync(IEnumerable<TDto> dtos); 
    }
}