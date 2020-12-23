using System.Threading.Tasks;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    
    public interface IVoidCommandHandler<in TCommand> 
        where TCommand : notnull
    {
        Task<Response> HandleAsync(TCommand command);
    }
}