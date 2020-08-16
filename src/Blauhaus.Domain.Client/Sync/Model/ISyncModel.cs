using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sync.Client;

namespace Blauhaus.Domain.Client.Sync.Model
{
    public interface ISyncModel<TModel> where TModel : IClientEntity
    {
        IObservable<TModel> Connect(Guid id);
        void LoadNewFromServer();
        void LoadNewFromClient();
        void ReloadFromClient();
    }
}