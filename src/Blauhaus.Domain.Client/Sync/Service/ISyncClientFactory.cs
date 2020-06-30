using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.Sync.Client;

namespace Blauhaus.Domain.Client.Sync.Service
{
    public interface ISyncClientFactory<TSyncCommand>
    {
        IList<Func<TSyncCommand, ClientSyncRequirement, ISyncStatusHandler, IObservable<object>>> SyncConnections { get; }
    }
}