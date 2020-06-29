using System;
using Blauhaus.Domain.Client.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestChildEntity : BaseSyncClientEntity
    {
        public Guid RootEntityId { get; set; }
        public string ChildName { get; set; }
    }
}