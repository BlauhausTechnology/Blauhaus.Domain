using Blauhaus.Domain.Abstractions.Sync;
using Newtonsoft.Json;

namespace Blauhaus.Domain.Client.Sync
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

        public static DtoSyncStatus Create(string dtoName, int currentDtoCount, int remainingDtoCount)
        {
            return new DtoSyncStatus(
                dtoName: dtoName, 
                currentDtoCount: currentDtoCount,
                totalDtoCount: currentDtoCount + remainingDtoCount,
                downloadedDtoCount: currentDtoCount, 
                remainingDtoCount: remainingDtoCount);
        }

        public DtoSyncStatus Update(int currentDtoCount, int remainingDtoCount) 
        {
            return new DtoSyncStatus(
                dtoName: DtoName,
                currentDtoCount: currentDtoCount,
                totalDtoCount: TotalDtoCount, 
                downloadedDtoCount: DownloadedDtoCount + currentDtoCount,
                remainingDtoCount: remainingDtoCount);
        }

        
        public override string ToString()
        {
            return $"Downloaded {CurrentDtoCount} entities of type {DtoName}. {DownloadedDtoCount} / {TotalDtoCount} downloaded so far";
        }
         
    }
}