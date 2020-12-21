using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{ 
    public interface IHandler<TPayload>
    {
        Task<Response<TPayload>> HandleAsync();
    }
}