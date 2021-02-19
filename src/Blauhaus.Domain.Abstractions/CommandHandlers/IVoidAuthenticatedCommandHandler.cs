using System.Threading.Tasks;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IVoidAuthenticatedCommandHandler<in TCommand, in TUser> 
        where TCommand : notnull
        where TUser : notnull
    {
        Task<Response> HandleAsync(TCommand command, TUser user);
    }
}