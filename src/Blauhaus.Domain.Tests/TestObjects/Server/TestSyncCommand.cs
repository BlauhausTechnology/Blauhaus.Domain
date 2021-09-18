using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Tests.TestObjects.Server
{
    public class TestSyncCommand : SyncCommand
    {
        public int RandomOtherFilterParameter { get; set; }
    }
}