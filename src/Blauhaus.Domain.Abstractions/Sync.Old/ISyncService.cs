using System;

namespace Blauhaus.Domain.Abstractions.Sync.Old
{
    public interface ISyncService
    {
        IObservable<SyncUpdate> Sync();
    }
}