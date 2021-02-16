using System;
using Blauhaus.Common.Utils.Contracts;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.TestHelpers.EntityBuilders
{
    public class BaseServerUserEntityBuilder<TBuilder, TEntity> : BaseServerEntityBuilder<TBuilder, TEntity> 
        where TEntity : class, IServerEntity, IHasUserId
        where TBuilder : BaseServerUserEntityBuilder<TBuilder, TEntity>
    {
        public BaseServerUserEntityBuilder(DateTime createdAt) : base(createdAt)
        {
        }
    }
}