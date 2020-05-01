using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers
{
    public class VoidCommandHandlerMockBuilder<TCommand> : BaseVoidCommandHandlerMockBuilder<VoidCommandHandlerMockBuilder<TCommand>, 
        IVoidCommandHandler<TCommand>, TCommand>
    {
        
    }
}
