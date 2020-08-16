using System;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Client.Sync.Client
{
    public interface ISyncClient<TModel, TSyncCommand> 
        where TModel : IClientEntity
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