using Blauhaus.Responses;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IGenericCommandHandler
    {
        Task<Response<TResponse>> HandleAsync<TCommand, TResponse>(TCommand command) where TCommand : notnull;
    }

    public interface IGenericCommandHandler<TResponse>
    {
        Task<Response<TResponse>> HandleAsync<TCommand>(TCommand command) where TCommand : notnull;
    }
}