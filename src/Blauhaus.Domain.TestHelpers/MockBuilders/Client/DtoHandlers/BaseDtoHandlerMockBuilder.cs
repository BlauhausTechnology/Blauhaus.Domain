using System;
using System.Linq.Expressions;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoHandlers
{
    public abstract class BaseDtoHandlerMockBuilder<TBuilder, TMock, TDto, TId> : BaseMockBuilder<TBuilder, TMock>
        where TBuilder : BaseDtoHandlerMockBuilder<TBuilder, TMock, TDto, TId> 
        where TMock : class, IDtoHandler<TDto>
        where TDto : class, IHasId<TId>
    {
        public void Verify_HandleAsync(params Expression<Func<TDto, bool>>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                Mock.Verify(x => x.HandleAsync(It.Is(predicate)));
            }
        }
    }
}