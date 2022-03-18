using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.TestHelpers.BaseTests;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.Time.TestHelpers.MockBuilders;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.TestHelpers.Builders.Base;
using NUnit.Framework;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Common.Abstractions;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{

    public abstract class BaseDbTest<TDbContext, TSut> : BaseServiceTest<TSut> 
        where TSut : class
        where TDbContext : DbContext 
    {
        private readonly TestDbProvider _provider;

        private IDbContextBuilder<TDbContext> _dbContextBuilder = null!;
        private readonly List<Action<TDbContext>> _entityFactories = new();   
        private TDbContext? _dbContextAfter;
        private TDbContext? _dbContextBefore;

        protected TDbContext GetNewDbContext() => _dbContextBuilder.NewContext;
        protected TDbContext DbContextBefore => _dbContextBefore ?? throw new InvalidOperationException("DbContextBefore is no longer valid once the test has started running");
        protected TDbContext DbContextAfter => _dbContextAfter ?? throw new InvalidOperationException("DbContextAfter is not valid until the test has finished running");

        protected virtual AnalyticsLoggerMockBuilder<TSut> MockLogger => AddMock<AnalyticsLoggerMockBuilder<TSut>, IAnalyticsLogger<TSut>>().Invoke();
        protected TimeServiceMockBuilder MockTimeService = null!;
        
        protected DateTime SetupTime;
        protected DateTime RunTime;

        protected BaseDbTest(TestDbProvider provider = TestDbProvider.InMemory)
        {
            _provider = provider;
        }
        
        [SetUp]
        public virtual void Setup()
        {
            base.Cleanup();
            
            AddService(MockLogger.Object);
            
            MockTimeService = new TimeServiceMockBuilder();
            AddService(MockTimeService.Object);

            _entityFactories.Clear();
            _dbContextBuilder = _provider switch
            {
                TestDbProvider.InMemory => new InMemoryDbContextBuilder<TDbContext>(),
                TestDbProvider.SqliteInMemory => new SqliteInMemoryDbContextBuilder<TDbContext>(),
                _ => _dbContextBuilder
            };
            
            _dbContextBefore = GetNewDbContext();

            SetupTime = MockTimeService.Reset();
            RunTime = SetupTime.AddSeconds(122);

            TDbContext FactoryFunc() => GetNewDbContext();
            AddService((Func<TDbContext>) FactoryFunc);
        }

        [TearDown]
        public virtual void Teardown()
        {
            _dbContextAfter?.Dispose();
            _dbContextBefore?.Dispose();
            _dbContextBuilder.Dispose();
        }

        protected override void BeforeConstructSut(IServiceProvider serviceProvider)
        {
            base.BeforeConstructSut(serviceProvider);

            foreach (var setupFunc in _entityFactories)
            {
                setupFunc.Invoke(DbContextBefore);
            }
            DbContextBefore.SaveChanges();
            MockTimeService.With(x => x.CurrentUtcTime, RunTime);
        }
         
        protected override async Task AfterConstructSutAsync(TSut sut)
        {
            if (typeof(TSut) is IAsyncInitializable asyncInitializable)
            {
                await asyncInitializable.InitializeAsync();
            }

            if (DbContextBefore.ChangeTracker.HasChanges())
            {
                DbContextBefore.SaveChanges();
            }
            _dbContextAfter = GetNewDbContext();
        }


        protected void AddEntityBuilders<T>(params IBuilder<T>[] builders) where T : BaseServerEntity 
        {
            foreach (var builder in builders)
            {
                _entityFactories.Add(context=> context.Add(builder.Object));
            } 
        }
        protected void AddEntityBuilder<T>(IBuilder<T> builder) where T : BaseServerEntity 
        {
            _entityFactories.Add(context=> context.Add(builder.Object));
        }

        protected void AddEntityBuilders<T>(List<IBuilder<T>> builders) where T : BaseServerEntity 
        {
            foreach (var builder in builders)
            {
                _entityFactories.Add(context=> context.Add(builder.Object));
            } 
        }

    }
}