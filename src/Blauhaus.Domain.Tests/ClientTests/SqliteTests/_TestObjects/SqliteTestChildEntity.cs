using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestChildEntity : SyncClientEntity<Guid> 
    {
        public Guid RootEntityId { get; set; }
        public string ChildName { get; set; }
    }
}