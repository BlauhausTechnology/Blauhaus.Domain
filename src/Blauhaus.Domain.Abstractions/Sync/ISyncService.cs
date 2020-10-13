using System;

namespace Blauhaus.Domain.Abstractions.Sync
{
    public interface ISyncService
    {
        IObservable<SyncUpdate> Sync();
    }
}