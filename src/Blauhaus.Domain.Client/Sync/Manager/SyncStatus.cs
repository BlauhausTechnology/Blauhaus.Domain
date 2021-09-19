using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.SyncClient;

namespace Blauhaus.Domain.Client.Sync.Manager
{
    public class SyncStatus : ISyncStatus
    {
        public SyncStatus(Dictionary<string, DtoSyncStatus> dtoStatuses)
        {
            DtoStatuses = dtoStatuses; 
        }

        public SyncStatus Update(DtoSyncStatus newDtoSyncStatus)
        {
            DtoStatuses[newDtoSyncStatus.DtoName] = newDtoSyncStatus;
            return this;
        }
        
        public Dictionary<string, DtoSyncStatus> DtoStatuses { get; }
         
        public int DownloadedDtoCount => DtoStatuses.Values.Sum(x => x.DownloadedDtoCount);
        public int TotalDtoCount => DtoStatuses.Values.Sum(x => x.TotalDtoCount);
        public float Progress => DownloadedDtoCount / (float)TotalDtoCount;

        
        public override string ToString()
        {
            return $"{TotalDtoCount} / {DownloadedDtoCount} [{Math.Round(Progress*100, 1)} %]";
        }
    }
}