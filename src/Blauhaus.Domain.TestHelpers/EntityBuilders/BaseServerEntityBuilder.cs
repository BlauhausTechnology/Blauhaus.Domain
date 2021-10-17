using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.Builders.Base;

namespace Blauhaus.Domain.TestHelpers.EntityBuilders
{

    public abstract class BaseServerEntityBuilder<TBuilder, TEntity> : BaseReadonlyFixtureBuilder<TBuilder, TEntity>
        where TEntity : class, IServerEntity
        where TBuilder : BaseServerEntityBuilder<TBuilder, TEntity>
    {

        public Guid Id { get; } 
        public DateTime CreatedAt { get; }

        protected BaseServerEntityBuilder(DateTime createdAt)
        {
            Id = Guid.NewGuid();
            CreatedAt = createdAt;

            With(x => x.CreatedAt, createdAt);
            With(x => x.ModifiedAt, createdAt);
            With(x => x.EntityState, EntityState.Active);
            With(x => x.Id, Id);
        }
    }
}