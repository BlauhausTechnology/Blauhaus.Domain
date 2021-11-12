using System;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Server.EFCore.Actors;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{
    public abstract class BaseDbModelActorTest<TDbContext, TActor, TModel> : BaseDbTest<TDbContext, TActor>
        where TDbContext : DbContext 
        where TActor : BaseDbModelActor<TDbContext, TModel>
        where TModel : IHasId<Guid>
    {
 
        protected Guid Id;

        public override void Setup()
        {
            base.Cleanup();
            
            Id = Guid.NewGuid();
        }

        protected override void AfterConstructSut(TActor sut)
        {
            base.AfterConstructSut(sut);
            
            Task.Run(async () => await sut.InitializeAsync(Id)).Wait();
        }
          

    }
}