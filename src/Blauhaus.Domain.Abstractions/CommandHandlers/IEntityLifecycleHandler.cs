using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Domain.Abstractions.Commands;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IEntityLifecycleHandler :
        IVoidAuthenticatedCommandHandler<ActivateCommand, IAuthenticatedUser>,
        IVoidAuthenticatedCommandHandler<ArchiveCommand, IAuthenticatedUser>,
        IVoidAuthenticatedCommandHandler<DeleteCommand, IAuthenticatedUser>
    {
    }
}