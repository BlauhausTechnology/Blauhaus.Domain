using Blauhaus.Sync.Abstractions;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestSyncCommand : SyncCommand
    {
        public string FavouriteFood { get; set; }
        public string FavouriteColour { get; set; }
    }
}