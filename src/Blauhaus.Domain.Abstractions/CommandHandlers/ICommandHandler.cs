using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Responses;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface ICommandHandler<TPayload, in TCommand> 
        where TCommand : notnull
    {
        Task<Response<TPayload>> HandleAsync(TCommand command, CancellationToken token);
    }

}