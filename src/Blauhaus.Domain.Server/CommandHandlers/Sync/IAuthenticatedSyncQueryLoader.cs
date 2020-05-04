using System.Linq;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Server.CommandHandlers.Sync
{
    public interface IAuthenticatedSyncQueryLoader<TEntity, TSyncQuery, TUser> : IAuthenticatedCommandHandler<IQueryable<TEntity>, TSyncQuery, TUser>
        where TSyncQuery : SyncCommand
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