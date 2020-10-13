using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.Repositories
{

    public interface ISyncClientRepository<TModel, in TDto> : ISyncClientRepository<TModel, TDto, SyncCommand>
        where TModel : class, IClientEntity
    {

    }


    public interface ISyncClientRepository<TModel, in TDto, TSyncCommand> : IClientRepository<TModel, TDto>
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
    {
        Task<ClientSyncStatus> GetSyncStatusAsync(TSyncCommand syncCommand);
        Task<IReadOnlyList<TModel>> LoadModelsAsync(TSyncCommand syncCommand);
        Task<IReadOnlyList<TModel>> SaveSyncedDtosAsync(IEnumerable<TDto> dtos); 
    }
}