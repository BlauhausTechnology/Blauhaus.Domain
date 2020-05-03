using System.Threading.Tasks;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.DeviceServices.Abstractions.DeviceInfo;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.TestHelpers.MockBuilders.ClientRepositoryHelpers;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.BaseTests;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base
{
    public class BaseSqliteTest<TSut> : BaseServiceTest<TSut> where TSut : class
    {
        protected ISqliteDatabaseService SqliteDatabaseService;
        protected SQLiteAsyncConnection Connection;

        [SetUp]
        public virtual void Setup()
        {
            Cleanup();
            SqliteDatabaseService = new SqliteDatabaseService(new SqliteConfig(), MockDeviceInfoService.Object);
            Task.Run(async () => await SqliteDatabaseService.DropTablesAsync()).Wait();
            Connection = SqliteDatabaseService.GetDatabaseConnectionAsync().Result;
            AddService(SqliteDatabaseService);
            AddService(x => MockClientRepositoryHelper.Object);
        }

        protected DeviceInfoServiceMockBuilder MockDeviceInfoService => AddMock<DeviceInfoServiceMockBuilder, IDeviceInfoService>().Invoke();

        protected ClientRepositoryHelperMockBuilder<ITestModel, TestRootEntity, ITestDto> MockClientRepositoryHelper => AddMock<ClientRepositoryHelperMockBuilder<ITestModel, TestRootEntity, ITestDto>,
            IClientRepositoryHelper<ITestModel, TestRootEntity, ITestDto>>().Invoke();
    }
}