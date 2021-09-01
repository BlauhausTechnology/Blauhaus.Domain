using System;

namespace Blauhaus.Domain.Abstractions.Entities
{
    
    public interface ISyncClientEntity<out TId> : IClientEntity<TId>, ISerializedDto
    {
        SyncState SyncState { get; set; }
    }
    
    public interface ISyncClientEntity : ISyncClientEntity<Guid>
    { 
    }
    
}