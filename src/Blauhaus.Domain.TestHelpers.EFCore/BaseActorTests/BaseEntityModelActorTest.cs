using System;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Server.EFCore.Actors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EntityBuilders;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{
    public abstract class BaseEntityModelActorTest<TDbContext, TActor, TModel, TEntity, TEntityBuilder> : BaseDbModelActorTest<TDbContext, TActor, TModel> 
        where TDbContext : DbContext 
        where TActor : BaseEntityModelActor<TDbContext, TEntity, TModel> 
        where TModel : IHasId<Guid>
        where TEntity : BaseServerEntity
        where TEntityBuilder : BaseServerEntityBuilder<TEntityBuilder, TEntity>
    {

        protected TEntityBuilder ExistingEntityBuilder = null!;
        protected TEntity ExistingEntity => ExistingEntityBuilder.Object;

        public override void Setup()
        {
            base.Setup();

            ExistingEntityBuilder = (TEntityBuilder)Activator.CreateInstance(typeof(TEntityBuilder), SetupTime)!;
            ExistingEntityBuilder.With(x => x.Id, Id);
            AddEntityBuilder(ExistingEntityBuilder);
        }
    }
}