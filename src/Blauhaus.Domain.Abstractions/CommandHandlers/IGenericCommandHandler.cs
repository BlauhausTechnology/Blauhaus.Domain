using Blauhaus.Responses;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IGenericCommandHandler
    {
        Task<Response<TResponse>> HandleCommandAsync<TCommand, TResponse>(TCommand command) where TCommand : notnull;
    }

    public interface IGenericCommandHandler<TResponse>
    {
        Task<Response<TResponse>> HandleCommandAsync<TCommand>(TCommand command) where TCommand : notnull;
    }
}