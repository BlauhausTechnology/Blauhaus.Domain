using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.Entities
{

    public class EntityMockBuilder : BaseEntityMockBuilder<EntityMockBuilder, IEntity>
    {
    }

    public class BaseEntityMockBuilder<TBuilder, TMock> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IEntity
        where TBuilder : BaseEntityMockBuilder<TBuilder, TMock>
    {
        public BaseEntityMockBuilder()
        {
            With(x => x.Id, Guid.NewGuid());
            With(x => x.EntityState, EntityState.Active);
        }
    }
}