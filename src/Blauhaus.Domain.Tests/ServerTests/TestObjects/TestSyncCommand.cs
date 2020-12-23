using Blauhaus.Domain.Abstractions.Sync;

namespace Blauhaus.Domain.Tests.ServerTests.TestObjects
{
    public class TestSyncCommand : SyncCommand
    {
        public int RandomOtherFilterParameter { get; set; }
    }
}