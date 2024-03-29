﻿using System;
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
            base.Setup();
            
            Id = Guid.NewGuid();
        }

        protected override async Task AfterConstructSutAsync(TActor sut)
        {
            await sut.InitializeAsync(Id);

            await base.AfterConstructSutAsync(sut);
        }
    }
}