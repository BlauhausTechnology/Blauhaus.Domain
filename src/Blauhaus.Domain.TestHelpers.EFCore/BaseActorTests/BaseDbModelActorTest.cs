using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Server.EFCore.Actors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.Builders.Base;
using Blauhaus.Time.TestHelpers.MockBuilders;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{
    public abstract class BaseDbModelActorTest<TDbContext, TActor, TModel> : BaseServiceTest<TActor>
        where TDbContext : DbContext 
        where TActor : BaseDbModelActor<TDbContext, TModel>
        where TModel : IHasId<Guid>
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

        protected Guid Id;


        [SetUp]
        public virtual void Setup()
        {
            base.Cleanup();
            
            Id = Guid.NewGuid();
            
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

        protected sealed override TActor ConstructSut()
        {
            foreach (var setupFunc in _entityFactories)
            {
                setupFunc.Invoke(DbContextBefore);
            }
            DbContextBefore.SaveChanges();
            _dbContextBefore = null;

            MockTimeService.With(x => x.CurrentUtcTime, RunTime);

            var sut = base.ConstructSut();
            Task.Run(async () => await sut.InitializeAsync(Id)).Wait();
            
            _dbContextAfter = GetNewDbContext();

            return sut;
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