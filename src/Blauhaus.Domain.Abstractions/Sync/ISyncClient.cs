using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.Sync
{
    public interface ISyncClient<TModel, TSyncCommand> 
        where TModel : IClientEntity<Guid>
        where TSyncCommand : SyncCommand
    {
        
        IObservable<TModel> Connect(TSyncCommand syncCommand, ClientSyncRequirement syncRequirement, ISyncStatusHandler syncStatusHandler);

        void LoadNextBatch();
        void LoadNewFromServer();
        void LoadNewFromClient();
        void ReloadFromClient();
        void Cancel();
    }
}