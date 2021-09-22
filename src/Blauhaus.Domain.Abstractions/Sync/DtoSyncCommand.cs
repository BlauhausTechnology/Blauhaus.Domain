namespace Blauhaus.Domain.Abstractions.Sync
{
    public class DtoSyncCommand
    {
        public DtoSyncCommand(string dtoName, long modifiedAfterTicks, bool isFirstSync)
        {
            ModifiedAfterTicks = modifiedAfterTicks;
            IsFirstSync = isFirstSync;
            DtoName = dtoName;
        }

        public string DtoName { get; }
        public long ModifiedAfterTicks { get; }
        public bool IsFirstSync { get; }


        public static DtoSyncCommand Create<T>(long lastModifiedTicks)
        {
            return new DtoSyncCommand(typeof(T).Name, lastModifiedTicks, lastModifiedTicks == 0);
        }

        public DtoSyncCommand Update(long lastModifiedTicks)
        {
            return new DtoSyncCommand(DtoName, lastModifiedTicks, IsFirstSync);
        }
    }
}