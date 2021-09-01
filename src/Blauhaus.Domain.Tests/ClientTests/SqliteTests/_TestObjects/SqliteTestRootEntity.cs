using System;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestRootEntity : SyncClientEntity<Guid>
    {

        public SqliteTestRootEntity()
        {
            Id = Guid.NewGuid();
            SyncState = SyncState.InSync;
        }

        public string RootName { get; set; }
    }
}