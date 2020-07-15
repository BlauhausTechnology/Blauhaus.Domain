using System;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.Model
{
    public class SyncModel<TModel, TSyncCommand> : ISyncModel<TModel> 
        where TModel : IClientEntity 
        where TSyncCommand : SyncCommand, new()
    {
        private readonly ISyncClient<TModel, TSyncCommand> _syncClient;

        public SyncModel(ISyncClient<TModel, TSyncCommand> syncClient)
        {
            _syncClient = syncClient;
        }

        public IObservable<TModel> Connect(Guid id)
        { 
            return _syncClient.Connect(new TSyncCommand
            {
                Id = id,
                BatchSize = 1
            }, ClientSyncRequirement.Minimum(1), new SyncStatusHandler());
        }

        public void LoadNewFromServer()
        {
            _syncClient.LoadNewFromServer();
        }
    }
}