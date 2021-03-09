using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
    public static class UserDbSetExtensions
    {
        
        public static async Task<TEntity?> LoadOneByUserIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.EntityState != Abstractions.Entities.EntityState.Deleted);
        }
        
        public static TEntity? LoadOneByUserId<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId
        {
            return dbSet.AsNoTracking().FirstOrDefault(x => x.UserId == userId && x.EntityState != Abstractions.Entities.EntityState.Deleted);
        }
            
        public static async Task<List<TEntity>> LoadByUserIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId
        {
            return await dbSet.AsNoTracking().Where(x => x.UserId == userId && x.EntityState!= Abstractions.Entities.EntityState.Deleted).ToListAsync();
        }
        
        public static List<TEntity> LoadByUserId<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId
        {
            return dbSet.AsNoTracking().Where(x => x.UserId == userId && x.EntityState!= Abstractions.Entities.EntityState.Deleted).ToList();
        }
        
        public static async Task<List<Guid>> LoadIdsByUserIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId
        {
            return await dbSet.AsNoTracking()
                .Where(x => x.UserId == userId && x.EntityState != Abstractions.Entities.EntityState.Deleted)
                .Select(x => x.Id)
                .ToListAsync();
        }

    }
}