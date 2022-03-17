using System;
using System.Collections.Generic;
using System.Text;

namespace Blauhaus.Domain.Abstractions.Entities
{
    public interface ISyncClientEntity<out TId> : IClientEntity<TId>
    {
        SyncState SyncState { get; set; }
    }
}
