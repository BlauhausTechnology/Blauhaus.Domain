using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface ICommandHandler<TPayload, TCommand> 
        where TCommand : notnull
    {
        Task<Result<TPayload>> HandleAsync(TCommand command, CancellationToken token);
    }

}