using System;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.ModelLoader
{
    public class ModelLoader<TModel, TSyncCommand> : IModelLoader<TModel, TSyncCommand> where TModel : IClientEntity where TSyncCommand : SyncCommand
    {
        private readonly ISyncClient<TModel, TSyncCommand> _syncClient;

        public ModelLoader(ISyncClient<TModel, TSyncCommand> syncClient)
        {
            _syncClient = syncClient;
        }


        public IObservable<TModel> Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}