using System;
using Blauhaus.Common.Abstractions;

namespace Blauhaus.Domain.Server.Entities
{
    public abstract class BaseChildEntity : IHasId
    {
        protected BaseChildEntity()
        {
        }

        protected BaseChildEntity(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}