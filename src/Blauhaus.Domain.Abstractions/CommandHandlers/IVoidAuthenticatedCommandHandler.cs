using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IVoidAuthenticatedCommandHandler<TCommand, TUser> 
        where TCommand : notnull
        where TUser : notnull
    {
        Task<Result> HandleAsync(TCommand command, TUser authenticatedUser, CancellationToken token);
    }
}