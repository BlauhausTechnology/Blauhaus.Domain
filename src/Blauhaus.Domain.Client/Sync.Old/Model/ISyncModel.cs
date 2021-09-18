using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Client.Sync.Old.Model
{
    public interface ISyncModel<TModel> where TModel : IClientEntity<Guid>
    {
        IObservable<TModel> Connect(Guid id);
        void LoadNewFromServer();
        void LoadNewFromClient();
        void ReloadFromClient();
    }
}