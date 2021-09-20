namespace Blauhaus.Domain.Abstractions.Sync
{
    public class DtoSyncCommand
    {
        public DtoSyncCommand(string dtoName, long modifiedAfterTicks)
        {
            ModifiedAfterTicks = modifiedAfterTicks;
            DtoName = dtoName;
        }

        public string DtoName { get; }
        public long ModifiedAfterTicks { get; }

        public static DtoSyncCommand Create<T>(long lastModifiedTicks)
        {
            return new DtoSyncCommand(typeof(T).Name, lastModifiedTicks);
        }
    }
}