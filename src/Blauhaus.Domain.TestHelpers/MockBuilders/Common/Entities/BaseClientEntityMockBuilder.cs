using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.Entities
{
    public class ClientEntityMockBuilder : BaseClientEntityMockBuilder<ClientEntityMockBuilder, IClientEntity>
    {
    }


    public abstract class BaseClientEntityMockBuilder<TBuilder, TMock> : BaseEntityMockBuilder<TBuilder, TMock> 
        where TBuilder : BaseClientEntityMockBuilder<TBuilder, TMock>
        where TMock : class, IClientEntity
    {
        protected BaseClientEntityMockBuilder()
        {
            With(x => x.Id, Guid.NewGuid());
            With(x => x.EntityState, EntityState.Active);
        }
    }
}