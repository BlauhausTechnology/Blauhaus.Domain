using System;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync
{
    public interface ISyncClient<TModel, TSyncCommand> 
        where TModel : IClientEntity
        where TSyncCommand : SyncCommand
    {
        IObservable<SyncUpdate<TModel>> Connect(TSyncCommand syncCommand);
    }
}