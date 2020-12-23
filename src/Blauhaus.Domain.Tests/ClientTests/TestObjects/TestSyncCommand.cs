using Blauhaus.Domain.Abstractions.Sync;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestSyncCommand : SyncCommand
    {
        public string FavouriteFood { get; set; }
        public string FavouriteColour { get; set; }
    }
}