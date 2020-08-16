using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestSyncCommand : SyncCommand
    {
        public string NameContains { get; set; }
    }
}