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
using Blauhaus.Common.Abstractions;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{
    public abstract class BaseDbTest<TDbContext, TSut> : BaseServiceTest<TSut> 
        where TSut : class
        where TDbContext : DbContext 
    {
        
        private InMemoryDbContextBuilder<TDbContext> _dbContextBuilder = null!;
        private readonly List<Action<TDbContext>> _entityFactories = new();   
        private TDbContext? _dbContextAfter;
        private TDbContext? _dbContextBefore;

        protected TDbContext GetNewDbContext() => _dbContextBuilder.NewContext;
        protected TDbContext DbContextBefore => _dbContextBefore ?? throw new InvalidOperationException("DbContextBefore is no longer valid once the test has started running");
        protected TDbContext DbContextAfter => _dbContextAfter ?? throw new InvalidOperationException("DbContextAfter is not valid until the test has finished running");

        protected AnalyticsServiceMockBuilder MockAnalyticsService = null!;
        protected TimeServiceMockBuilder MockTimeService = null!;
        
        protected DateTime SetupTime;
        protected DateTime RunTime;


        
        [SetUp]
        public virtual void Setup()
        {
            base.Cleanup();
            
            MockAnalyticsService = new AnalyticsServiceMockBuilder();
            AddService(MockAnalyticsService.Object);
            
            MockTimeService = new TimeServiceMockBuilder();
            AddService(MockTimeService.Object);

            _entityFactories.Clear();
            _dbContextBuilder = new InMemoryDbContextBuilder<TDbContext>();

            _dbContextAfter = null;
            _dbContextBefore = GetNewDbContext();

            SetupTime = MockTimeService.Reset();
            RunTime = SetupTime.AddSeconds(122);

            TDbContext FactoryFunc() => GetNewDbContext();
            AddService((Func<TDbContext>) FactoryFunc);
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
            
            DbContextBefore.SaveChanges();
            _dbContextBefore = null;
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