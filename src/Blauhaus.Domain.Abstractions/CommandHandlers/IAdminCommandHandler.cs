using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Domain.Abstractions.Commands;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IAdminCommandHandler<in TCommand> : IVoidAuthenticatedCommandHandler<TCommand, IAuthenticatedUser> 
        where TCommand : IAdminCommand
    {
    }
     
    public interface IAdminCommandHandler<TResponse, in TCommand> : IAuthenticatedCommandHandler<TResponse, TCommand, IAuthenticatedUser> 
        where TCommand : IAdminCommand
    {
    }

}