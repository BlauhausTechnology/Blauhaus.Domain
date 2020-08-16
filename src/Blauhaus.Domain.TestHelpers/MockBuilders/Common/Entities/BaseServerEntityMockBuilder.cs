using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.Entities
{
    public class ServerEntityMockBuilder : BaseServerEntityMockBuilder<ServerEntityMockBuilder, IServerEntity>
    {
    }


    public abstract class BaseServerEntityMockBuilder<TBuilder, TMock> : BaseEntityMockBuilder<TBuilder, TMock> 
        where TBuilder : BaseServerEntityMockBuilder<TBuilder, TMock>
        where TMock : class, IServerEntity
    {
        protected BaseServerEntityMockBuilder()
        {
            With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-2));
            With(x => x.ModifiedAt, DateTime.UtcNow.AddDays(-1));
        }
    }
}