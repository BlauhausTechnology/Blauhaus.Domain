using System;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Server.EFCore.Actors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EntityBuilders;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.BaseActorTests
{
    public abstract class BaseEntityDtoModelActorTest<TDbContext, TActor, TModel, TEntity, TEntityBuilder, TDto> : BaseEntityModelActorTest<TDbContext, TActor, TModel, TEntity, TEntityBuilder> 
        where TDbContext : DbContext 
        where TActor : BaseEntityDtoModelActor<TDbContext, TEntity, TModel, TDto> 
        where TModel : IHasId<Guid>
        where TEntity : BaseServerEntity, IDtoOwner<TDto>
        where TEntityBuilder : BaseServerEntityBuilder<TEntityBuilder, TEntity>
        where TDto : IHasId<Guid>
    {
        
    }
}