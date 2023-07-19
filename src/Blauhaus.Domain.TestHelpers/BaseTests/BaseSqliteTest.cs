using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.ClientDatabase.Sqlite.Config;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.DeviceServices.Abstractions.DeviceInfo;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.Builders.Base;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.TestHelpers.BaseTests
{
    public abstract class BaseSqliteTest<TSut, TSqliteConfig>: BaseServiceTest<TSut> 
        where TSut : class
        where TSqliteConfig : BaseSqliteConfig
    {
        
        private readonly List<Action<SQLiteAsyncConnection>> _entityFactories = new();   
        protected ISqliteDatabaseService SqliteDatabaseService = null!;
        protected SQLiteAsyncConnection Connection = null!;

        [SetUp]
        public virtual void Setup()
        {
            base.Cleanup();
            
            _entityFactories.Clear();

            var config = (ISqliteConfig)Activator.CreateInstance(typeof(TSqliteConfig), MockDeviceInfoService.Object)!;
            SqliteDatabaseService = new SqliteInMemoryDatabaseService(MockSqliteLogger.Object, MockKeyValueStore.Object, config);
            Task.Run(async () => await SqliteDatabaseService.DeleteDataAsync()).Wait();
            Connection = SqliteDatabaseService.AsyncConnection;
            
            AddService(MockDeviceInfoService.Object);
            AddService(MockAnalyticsService.Object);
            AddService(SqliteDatabaseService);
            AddService(MockLogger.Object);
            AddService(MockSqliteLogger.Object);
            AddService(MockKeyValueStore.Object);
        }

        protected DeviceInfoServiceMockBuilder MockDeviceInfoService => AddMock<DeviceInfoServiceMockBuilder, IDeviceInfoService>().Invoke();
        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
        private AnalyticsLoggerMockBuilder<TSut> MockLogger => AddMock<AnalyticsLoggerMockBuilder<TSut>, IAnalyticsLogger<TSut>>().Invoke();
        private AnalyticsLoggerMockBuilder<SqliteInMemoryDatabaseService> MockSqliteLogger => AddMock<AnalyticsLoggerMockBuilder<SqliteInMemoryDatabaseService>, IAnalyticsLogger<SqliteInMemoryDatabaseService>>().Invoke();
        protected KeyValueStoreMockBuilder MockKeyValueStore => AddMock<KeyValueStoreMockBuilder, IKeyValueStore>().Invoke();

        protected override void BeforeConstructSut(IServiceProvider serviceProvider)
        {
            base.BeforeConstructSut(serviceProvider);

            foreach (var setupFunc in _entityFactories)
            {
                setupFunc.Invoke(Connection);
            }
        }
        protected void ClearEntityBuilders() => _entityFactories.Clear();
        
        protected void AddEntityBuilders<T>(params IBuilder<T>[] builders) 
        {
            foreach (var builder in builders)
            {
                _entityFactories.Add(connection=> Task.Run(async () =>
                {
                    await connection.InsertAsync(builder.Object);
                }).Wait());
            } 
        }
        protected void AddEntityBuilder<T>(IBuilder<T> builder) 
        {
            _entityFactories.Add(connection=> Task.Run(async ()=> await connection.InsertAsync(builder.Object)).Wait());
        }

        protected void AddEntityBuilders<T>(List<IBuilder<T>> builders)  
        {
            foreach (var builder in builders)
            {
                _entityFactories.Add(connection=> Task.Run(async ()=> await connection.InsertAsync(builder.Object)).Wait());
            } 
        }
    }
}