using System;
using System.Collections.Generic;

namespace Blauhaus.Domain.Abstractions.Sync
{
    public interface ISyncClientFactory<TSyncCommand>
    {
        IList<Func<TSyncCommand, ClientSyncRequirement, ISyncStatusHandler, IObservable<object>>> SyncConnections { get; }
    }
}