using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.Builders.Base;

namespace Blauhaus.Domain.TestHelpers.EntityBuilders
{

    public abstract class BaseServerEntityBuilder<TBuilder, TEntity> : BaseReadonlyFixtureBuilder<TBuilder, TEntity>
        where TEntity : class, IServerEntity
        where TBuilder : BaseServerEntityBuilder<TBuilder, TEntity>
    {

        public Guid Id { get; private set; } 
        public DateTime CreatedAt { get; private set; }

        protected BaseServerEntityBuilder(DateTime createdAt)
        {
            Id = Guid.NewGuid();
            CreatedAt = createdAt;

            With(x => x.CreatedAt, createdAt);
            With(x => x.ModifiedAt, createdAt);
            With(x => x.EntityState, EntityState.Active);
            With(x => x.Id, Id);
        }

        public virtual TBuilder WithId(Guid id)
        {
            Id = id;
            With(x => x.Id, id);
            return (TBuilder)this;
        }
    }
}