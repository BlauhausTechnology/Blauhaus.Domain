using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Commands;
using Blauhaus.Domain.Abstractions.Dtos;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IGetCommandHandler<TDto, in TUser>
    {
        Task<GetResultDto<TDto>> GetAsync(GetCommand command, TUser user);
    }
}