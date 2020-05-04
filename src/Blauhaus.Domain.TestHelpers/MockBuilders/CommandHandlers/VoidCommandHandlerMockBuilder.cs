using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers
{
    public class VoidCommandHandlerMockBuilder<TCommand> : VoidCommandHandlerMockBuilder<VoidCommandHandlerMockBuilder<TCommand>, 
        IVoidCommandHandler<TCommand>, TCommand>
    {
        
    }
}
