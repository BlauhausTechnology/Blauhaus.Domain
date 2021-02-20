using System.Threading.Tasks;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IVoidAuthenticatedCommandHandler<in TCommand> : IVoidAuthenticatedCommandHandler<TCommand, IAuthenticatedUser>
        where TCommand : notnull
    {
    } 
    
    public interface IVoidAuthenticatedCommandHandler<in TCommand, in TUser> 
        where TCommand : notnull
        where TUser : notnull
    {
        Task<Response> HandleAsync(TCommand command, TUser user);
    }
}