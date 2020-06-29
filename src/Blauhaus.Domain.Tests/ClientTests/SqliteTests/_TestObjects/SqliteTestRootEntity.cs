using System;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestRootEntity : BaseSyncClientEntity
    {

        public SqliteTestRootEntity()
        {
            Id = Guid.NewGuid();
            SyncState = SyncState.InSync;
        }

        public string RootName { get; set; }
    }
}