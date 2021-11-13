using System;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Server.EFCore.Actors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EntityBuilders;
using Blauhaus.Domain.TestHelpers.Extensions;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoHandlers;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{
    public abstract class BaseEntityDtoModelActorTest<TDbContext, TActor, TModel, TEntity, TEntityBuilder, TDto> : BaseEntityModelActorTest<TDbContext, TActor, TModel, TEntity, TEntityBuilder> 
        where TDbContext : DbContext 
        where TActor : BaseEntityDtoModelActor<TDbContext, TEntity, TModel, TDto> 
        where TModel : IHasId<Guid>
        where TEntity : BaseServerEntity, IDtoOwner<TDto>
        where TEntityBuilder : BaseServerEntityBuilder<TEntityBuilder, TEntity>
        where TDto : class, IHasId<Guid>
    {
        public override void Setup()
        {
            base.Setup();

            AddService(x => MockDtoHandler.Object);
        }

        protected DtoHandlerMockBuilder<TDto, Guid> MockDtoHandler => Mocks.AddMockDtoHandler<TDto, Guid>().Invoke();
    }
}