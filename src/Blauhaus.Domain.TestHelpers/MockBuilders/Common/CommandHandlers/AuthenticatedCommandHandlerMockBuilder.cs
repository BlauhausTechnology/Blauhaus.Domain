using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers
{
    public class AuthenticatedCommandHandlerMockBuilder<TPayload, TCommand, TUser> : AuthenticatedCommandHandlerMockBuilder<AuthenticatedCommandHandlerMockBuilder<TPayload, TCommand, TUser>, IAuthenticatedCommandHandler<TPayload, TCommand, TUser>, TPayload, TCommand, TUser> 
        where TCommand : notnull
        where TUser : notnull
    {
        
    }
}
