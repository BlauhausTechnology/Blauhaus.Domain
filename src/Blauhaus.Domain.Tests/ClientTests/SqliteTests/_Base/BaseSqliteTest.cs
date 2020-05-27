using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.DeviceServices.Abstractions.DeviceInfo;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientRepositoryHelpers;
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
            AddService(x => MockClientEntityConverter.Object);
            AddService(x => MockSyncClientSqlQueryGenerator.Object);
            AddService(x => MockAnalyticsService.Object);
        }

        protected DeviceInfoServiceMockBuilder MockDeviceInfoService => AddMock<DeviceInfoServiceMockBuilder, IDeviceInfoService>().Invoke();

        protected ClientEntityConverterMockBuilder<ITestModel, ITestDto, TestRootEntity> MockClientEntityConverter 
            => AddMock<ClientEntityConverterMockBuilder<ITestModel, ITestDto, TestRootEntity>, IClientEntityConverter<ITestModel, ITestDto, TestRootEntity>>().Invoke();

        protected SyncClientSqlQueryGeneratorMockBuilder<ISyncClientSqlQueryGenerator<TestSyncCommand, TestRootEntity>, TestRootEntity, TestSyncCommand> MockSyncClientSqlQueryGenerator
            => AddMock<SyncClientSqlQueryGeneratorMockBuilder<ISyncClientSqlQueryGenerator<TestSyncCommand, TestRootEntity>,TestRootEntity,  TestSyncCommand>, ISyncClientSqlQueryGenerator<TestSyncCommand, TestRootEntity>>().Invoke();

        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
    }
}