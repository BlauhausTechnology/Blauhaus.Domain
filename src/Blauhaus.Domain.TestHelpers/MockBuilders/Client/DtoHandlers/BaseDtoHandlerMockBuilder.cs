using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoHandlers
{
    public class BaseDtoHandlerMockBuilder<TBuilder, TMock, TDto, TId> : BaseMockBuilder<TBuilder, TMock>
        where TBuilder : BaseDtoHandlerMockBuilder<TBuilder, TMock, TDto, TId> 
        where TMock : class, IDtoHandler<TDto>
        where TDto : class, IHasId<TId>
    {
        
    }
}