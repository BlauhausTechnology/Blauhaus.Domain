using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers
{
    public class CommandHandlerMockBuilder<TPayload, TCommand> : BaseCommandHandlerMockBuilder<CommandHandlerMockBuilder<TPayload, TCommand>, 
        ICommandHandler<TPayload, TCommand>, TPayload, TCommand>
    {
        
    }
}
