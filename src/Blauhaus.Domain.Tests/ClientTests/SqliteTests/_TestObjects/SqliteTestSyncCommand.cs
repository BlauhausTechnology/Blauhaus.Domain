using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestSyncCommand : SyncCommand
    {
        public string NameContains { get; set; }
    }
}