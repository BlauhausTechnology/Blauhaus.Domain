using System.Linq;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Server.CommandHandlers.Sync
{
    public interface IAuthenticatedSyncQueryLoader<TEntity, TSyncCommand, TUser> : IAuthenticatedCommandHandler<IQueryable<TEntity>, TSyncCommand, TUser>
        where TSyncCommand : SyncCommand
        where TUser : notnull
        where TEntity : IEntity
    {
    }

    public interface IAuthenticatedSyncQueryLoader<TEntity, TUser> : IAuthenticatedSyncQueryLoader<TEntity, SyncCommand, TUser>
        where TUser : notnull 
        where TEntity : IServerEntity
    {
    }
}