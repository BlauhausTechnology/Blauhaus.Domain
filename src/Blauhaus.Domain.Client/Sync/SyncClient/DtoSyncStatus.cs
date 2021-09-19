using System;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.DtoBatches;
using Newtonsoft.Json;

namespace Blauhaus.Domain.Client.Sync.SyncClient
{
    public class DtoSyncStatus 
    {
        [JsonConstructor]
        public DtoSyncStatus(
            string dtoName, 
            int currentDtoCount, 
            int totalDtoCount, 
            int downloadedDtoCount, 
            int remainingDtoCount)
        {
            DtoName = dtoName; 

            TotalDtoCount = totalDtoCount;
            CurrentDtoCount = currentDtoCount;
            DownloadedDtoCount = downloadedDtoCount;
            RemainingDtoCount = remainingDtoCount;
        }
        
        public string DtoName { get; } 

        public int TotalDtoCount { get; }
        public int CurrentDtoCount { get; }
        public int DownloadedDtoCount { get; }
        public int RemainingDtoCount { get; }
        
        public float Progress => DownloadedDtoCount / (float)TotalDtoCount;


        public static DtoSyncStatus Create(string dtoName, IDtoBatch dtoBatch)
        {
            return new DtoSyncStatus(
                dtoName: dtoName,
                currentDtoCount: dtoBatch.CurrentDtoCount,
                totalDtoCount: dtoBatch.CurrentDtoCount + dtoBatch.RemainingDtoCount,
                downloadedDtoCount: dtoBatch.CurrentDtoCount,
                remainingDtoCount: dtoBatch.RemainingDtoCount);
        }

        public DtoSyncStatus Update(IDtoBatch dtoBatch) 
        {
            return new DtoSyncStatus(
                dtoName: DtoName,
                currentDtoCount: dtoBatch.CurrentDtoCount,
                totalDtoCount: TotalDtoCount, 
                downloadedDtoCount: DownloadedDtoCount + dtoBatch.CurrentDtoCount,
                remainingDtoCount: dtoBatch.RemainingDtoCount);
        }

        
        public override string ToString()
        {
            return $"Downloaded {CurrentDtoCount} entities of type {DtoName}. {DownloadedDtoCount} / {TotalDtoCount} downloaded so far";
        }
         
    }
}