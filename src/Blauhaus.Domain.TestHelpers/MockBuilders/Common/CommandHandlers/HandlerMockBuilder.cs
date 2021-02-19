using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers
{
    public class HandlerMockBuilder<TPayload> : HandlerMockBuilder<HandlerMockBuilder<TPayload>, IHandler<TPayload>, TPayload>
    {
        
    }
}