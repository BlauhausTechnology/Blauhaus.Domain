using System;
using Blauhaus.ClientDatabase.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class TestChildEntity : BaseSqliteEntity
    {
        public Guid RootEntityId { get; set; }
        public string ChildName { get; set; }
    }
}