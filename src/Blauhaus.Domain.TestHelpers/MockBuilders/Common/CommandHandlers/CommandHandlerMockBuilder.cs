﻿using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers
{
    public class CommandHandlerMockBuilder<TPayload, TCommand> 
        : CommandHandlerMockBuilder<
            CommandHandlerMockBuilder<TPayload, TCommand>, 
            ICommandHandler<TPayload, TCommand>, 
            TPayload, 
            TCommand> where TCommand : notnull
    {
        
    }
}
