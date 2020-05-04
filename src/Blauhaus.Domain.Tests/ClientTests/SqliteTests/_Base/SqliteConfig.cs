using System;
using System.Collections.Generic;
using Blauhaus.ClientDatabase.Sqlite.Config;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base
{
    public class SqliteConfig : ISqliteConfig
    {
        public SqliteConfig()
        {
            DatabaseName = "SqliteTestDb";
            TableTypes = new List<Type>
            {
                typeof(TestRootEntity),
                typeof(TestChildEntity),
                typeof(TestGrandChildEntity),
            };
        }

        public string DatabaseName { get; }
        public IList<Type> TableTypes { get; }
    }
}