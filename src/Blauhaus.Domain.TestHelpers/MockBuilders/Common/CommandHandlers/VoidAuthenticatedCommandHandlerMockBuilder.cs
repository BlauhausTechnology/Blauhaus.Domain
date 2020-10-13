using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers
{
    public class VoidAuthenticatedCommandHandlerMockBuilder<TCommand, TUser> 
        : VoidAuthenticatedCommandHandlerMockBuilder<
            VoidAuthenticatedCommandHandlerMockBuilder<TCommand, TUser>, 
        IVoidAuthenticatedCommandHandler<TCommand, TUser>, TCommand, TUser> where TCommand : notnull
    {
        
    }
}
