using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.DtoHandlers;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoHandlers
{
    public class DtoHandlerMockBuilder<TDto, TId> : BaseDtoHandlerMockBuilder<DtoHandlerMockBuilder<TDto, TId>, IDtoHandler<TDto>, TDto, TId>
        where TDto : class, IHasId<TId>
    {
        
    }
}