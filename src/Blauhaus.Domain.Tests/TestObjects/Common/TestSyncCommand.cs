using Reveye.Common.Sync;

namespace Blauhaus.Domain.Tests.TestObjects.Common
{
    public class TestSyncCommand : SyncCommand
    {
        public string FavouriteFood { get; set; }
        public string FavouriteColour { get; set; }
    }
}