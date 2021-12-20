using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.ClientDatabase.Sqlite.Config;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.DeviceServices.Abstractions.DeviceInfo;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.TestHelpers.BaseTests;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.TestHelpers.BaseTests
{
    public abstract class BaseSqliteTest<TSut, TSqliteConfig>: BaseServiceTest<TSut> 
        where TSut : class
        where TSqliteConfig : BaseSqliteConfig
    {
        protected ISqliteDatabaseService SqliteDatabaseService = null!;
        protected SQLiteAsyncConnection Connection = null!;

        [SetUp]
        public virtual void Setup()
        {
            base.Cleanup();

            var config = (ISqliteConfig)Activator.CreateInstance(typeof(TSqliteConfig), MockDeviceInfoService.Object);
            SqliteDatabaseService = new SqliteInMemoryDatabase(config);
            Task.Run(async () => await SqliteDatabaseService.DeleteDataAsync()).Wait();
            Connection = SqliteDatabaseService.AsyncConnection;
            
            AddService(MockDeviceInfoService.Object);
            AddService(MockAnalyticsService.Object);
            AddService(SqliteDatabaseService);
        }

        protected DeviceInfoServiceMockBuilder MockDeviceInfoService => AddMock<DeviceInfoServiceMockBuilder, IDeviceInfoService>().Invoke();
        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
    }
}