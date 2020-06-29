using System;
using Blauhaus.Domain.Common.CommandHandlers.Sync;

namespace Blauhaus.Domain.Client.Sync.Service
{
    public interface ISyncService
    {
        IObservable<SyncUpdate> Sync();
    }
}