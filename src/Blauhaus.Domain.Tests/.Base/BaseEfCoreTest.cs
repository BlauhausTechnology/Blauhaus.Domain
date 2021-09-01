using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Microsoft.EntityFrameworkCore;
using System;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Tests.Base
{
    public abstract class BaseEfCoreTest<TSut, TDbContext> : BaseDomainTest<TSut> where TSut : class
        where TDbContext : DbContext

    {
        private InMemoryDbContextBuilder<TDbContext> _dbContextBuilder = null!;

        public override void Setup()
        {
            _dbContextBuilder = new InMemoryDbContextBuilder<TDbContext>();

            TDbContext FactoryFunc() => _dbContextBuilder.NewContext;
            Services.AddSingleton<Func<TDbContext>>(FactoryFunc);

            using (var setupContext = _dbContextBuilder.NewContext)
            {
                //a different context for test setup
                SetupDbContext(setupContext);
                setupContext.SaveChanges();
            }

            //and a different one for test assertions
            PostDbContext = _dbContextBuilder.NewContext;
        } 

        protected abstract void SetupDbContext(TDbContext setupContext);
        
        protected TDbContext PostDbContext = null!;

        protected T Seed<T>(T entity)
        {
            AdditionalSetup(context =>
            {
                context.Seed(entity);
            });
            return entity;
        }
        
        protected void AdditionalSetup(Action<TDbContext> setupFunc)
        {
            using (var dbContext =  _dbContextBuilder.NewContext)
            {
                setupFunc.Invoke(dbContext);
                dbContext.SaveChanges();
            }
        }
    }
}