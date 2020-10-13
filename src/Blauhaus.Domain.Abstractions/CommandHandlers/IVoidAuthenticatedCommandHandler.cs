using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Responses;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IVoidAuthenticatedCommandHandler<TCommand, TUser> 
        where TCommand : notnull
        where TUser : notnull
    {
        Task<Response> HandleAsync(TCommand command, TUser authenticatedUser, CancellationToken token);
    }
}