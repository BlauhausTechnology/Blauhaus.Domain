using System;
using Blauhaus.Common.Abstractions;

namespace Blauhaus.Domain.Server.Entities
{
    public abstract class BaseChildEntity : BaseChildEntity<Guid>
    {
        protected BaseChildEntity()
        {
        }

        protected BaseChildEntity(Guid id) : base(id)
        {
        }
    }

    public abstract class BaseChildEntity<TId> : IHasId<TId>
    {
        protected BaseChildEntity()
        {
        }

        protected BaseChildEntity(TId id)
        {
            Id = id;
        }

        public TId Id { get; private set; } = default!;
    }
}