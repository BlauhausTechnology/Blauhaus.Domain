﻿using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers
{
    public class VoidCommandHandlerMockBuilder<TCommand> : VoidCommandHandlerMockBuilder<VoidCommandHandlerMockBuilder<TCommand>, 
        IVoidCommandHandler<TCommand>, TCommand>
        where TCommand : notnull
    {
        
    }
}
