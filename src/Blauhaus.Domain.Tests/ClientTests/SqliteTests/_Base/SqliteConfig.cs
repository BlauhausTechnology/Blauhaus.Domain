using System;
using System.Collections.Generic;
using System.IO;
using Blauhaus.ClientDatabase.Sqlite.Config;
using Blauhaus.DeviceServices.Abstractions.DeviceInfo;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base
{
    public class SqliteConfig : ISqliteConfig
    {
        public SqliteConfig(IDeviceInfoService deviceInfoService)
        {
            DatabasePath = Path.Combine(deviceInfoService.AppDataFolder, "SqliteTestDb.sqlite");

            TableTypes = new List<Type>
            {
                typeof(SqliteTestRootEntity),
                typeof(SqliteTestChildEntity),
                typeof(SqliteTestGrandChildEntity),
                typeof(MyCachedDtoEntity),
            };
        }

        public IList<Type> TableTypes { get; }
        public string DatabasePath { get; }
    }
}