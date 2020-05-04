using Blauhaus.Domain.Common.CommandHandlers.Sync;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class TestSyncCommand : SyncCommand
    {
        public string NameContains { get; set; }
    }
}