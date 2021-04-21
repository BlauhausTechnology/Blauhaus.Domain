using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.Builders.Base;

namespace Blauhaus.Domain.TestHelpers.EntityBuilders
{

    public abstract class BaseServerEntityBuilder<TBuilder, TEntity> : BaseReadonlyFixtureBuilder<TBuilder, TEntity>
        where TEntity : class, IServerEntity
        where TBuilder : BaseServerEntityBuilder<TBuilder, TEntity>
    {

        protected Guid Id = Guid.NewGuid();

        protected BaseServerEntityBuilder(DateTime createdAt)
        {
            With(x => x.CreatedAt, createdAt);
            With(x => x.ModifiedAt, createdAt);
            With(x => x.EntityState, EntityState.Active);
            With(x => x.Id, Id);
        }
    }
}