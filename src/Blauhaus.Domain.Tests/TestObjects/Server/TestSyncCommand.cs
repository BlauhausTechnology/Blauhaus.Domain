using Blauhaus.Domain.Abstractions.Sync;

namespace Blauhaus.Domain.Tests.TestObjects.Server
{
    public class TestSyncCommand : SyncCommand
    {
        public int RandomOtherFilterParameter { get; set; }
    }
}