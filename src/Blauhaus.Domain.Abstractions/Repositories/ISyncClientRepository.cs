using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;
using SyncCommand = Blauhaus.Domain.Abstractions.Sync.Old.SyncCommand;

namespace Blauhaus.Domain.Abstractions.Repositories
{

    public interface ISyncClientRepository<TModel, in TDto> : ISyncClientRepository<TModel, TDto, SyncCommand>
        where TModel : class, IClientEntity<Guid>
    {

    }


    public interface ISyncClientRepository<TModel, in TDto, TSyncCommand> : IClientRepository<TModel, TDto>
        where TModel : class, IClientEntity<Guid>
        where TSyncCommand : SyncCommand
    {
        Task<ClientSyncStatus> GetSyncStatusAsync(TSyncCommand syncCommand);
        Task<IReadOnlyList<TModel>> LoadModelsAsync(TSyncCommand syncCommand);
        Task<IReadOnlyList<TModel>> SaveSyncedDtosAsync(IEnumerable<TDto> dtos); 
    }
}