using System;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.TestHelpers.Builders.Base;

namespace Blauhaus.Domain.TestHelpers.EntityBuilders
{
    public abstract class BaseChildEntityBuilder<TBuilder, TEntity> : BaseReadonlyFixtureBuilder<TBuilder, TEntity>
        where TBuilder : BaseChildEntityBuilder<TBuilder, TEntity>
        where TEntity : BaseChildEntity
    {

        public Guid Id { get; }

        protected BaseChildEntityBuilder()
        {
            Id = Guid.NewGuid();
            With(x => x.Id, Id);
        }
    }
}