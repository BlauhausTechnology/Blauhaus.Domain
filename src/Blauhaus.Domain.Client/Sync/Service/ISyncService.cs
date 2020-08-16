using System;

namespace Blauhaus.Domain.Client.Sync.Service
{
    public interface ISyncService
    {
        IObservable<SyncUpdate> Sync();
    }
}