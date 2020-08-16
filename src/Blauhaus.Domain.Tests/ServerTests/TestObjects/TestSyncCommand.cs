using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Server.CommandHandlers;

namespace Blauhaus.Domain.Tests.ServerTests.TestObjects
{
    public class TestSyncCommand : SyncCommand
    {
        public int RandomOtherFilterParameter { get; set; }
    }
}