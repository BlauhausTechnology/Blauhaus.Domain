using System;
using System.Linq;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Abstractions.Sync.Old;

namespace Blauhaus.Domain.Server.CommandHandlers.Sync
{
    public interface IAuthenticatedSyncQueryLoader<TEntity, in TSyncCommand, in TUser> : IAuthenticatedCommandHandler<IQueryable<TEntity>, TSyncCommand, TUser>
        where TSyncCommand : SyncCommand
        where TUser : notnull
        where TEntity : IEntity<Guid>
    {
    }

    public interface IAuthenticatedSyncQueryLoader<TEntity, in TUser> : IAuthenticatedSyncQueryLoader<TEntity, SyncCommand, TUser>
        where TUser : notnull 
        where TEntity : IServerEntity
    {
    }
}