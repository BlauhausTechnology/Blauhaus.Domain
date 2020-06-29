using System;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.ModelLoader
{
    public interface IModelLoader<TModel, TSyncCommand>
        where TModel : IClientEntity
        where TSyncCommand : SyncCommand
    {
        IObservable<TModel> Load(Guid id);
    }
}