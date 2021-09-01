using Blauhaus.Domain.Server.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.DbMappings
{
    public abstract class BaseSyncDbMapping<TEntity, TId> : BaseDbMapping<TEntity, TId>
        where TEntity : BaseSyncServerEntity<TId>
    {
        protected BaseSyncDbMapping(ModelBuilder modelBuilder) : base(modelBuilder)
        { 
        }
    }
}