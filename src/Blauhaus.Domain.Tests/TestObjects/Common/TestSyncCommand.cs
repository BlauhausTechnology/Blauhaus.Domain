using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Tests.TestObjects.Common
{
    public class TestSyncCommand : SyncCommand
    {
        public string FavouriteFood { get; set; }
        public string FavouriteColour { get; set; }
    }
}