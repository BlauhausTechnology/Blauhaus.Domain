using System;
using Blauhaus.ClientDatabase.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class TestRootEntity : BaseSqliteEntity
    {

        public TestRootEntity()
        {
            Id = Guid.NewGuid();
        }

        public string RootName { get; set; }
    }
}