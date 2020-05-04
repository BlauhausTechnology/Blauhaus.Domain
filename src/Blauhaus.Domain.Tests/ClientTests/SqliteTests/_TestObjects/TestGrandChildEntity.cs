using System;
using Blauhaus.ClientDatabase.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class TestGrandChildEntity: BaseSqliteEntity
    {
        public Guid ChildEntityId { get; set; }
        public string GrandchildName { get; set; }
    }
}