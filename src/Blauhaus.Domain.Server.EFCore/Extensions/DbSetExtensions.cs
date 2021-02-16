using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Common.Utils.Contracts;
using Blauhaus.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
   public static class DbSetExtensions
    {
        public static async Task<TEntity?> LoadByIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid id) where TEntity : class, IServerEntity
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.EntityState != EntityState.Deleted);
        }


        public static TEntity? LoadById<TEntity>(this DbSet<TEntity> dbSet, Guid id) where TEntity : class, IServerEntity
        {
            return dbSet.AsNoTracking().FirstOrDefault(x => x.Id == id && x.EntityState != EntityState.Deleted);
        }
                 
        public static async Task<TEntity?> LoadOneByUserIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId<Guid>
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.EntityState != EntityState.Deleted);
        }
        
        public static TEntity? LoadOneByUserId<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId<Guid>
        {
            return dbSet.AsNoTracking().FirstOrDefault(x => x.UserId == userId && x.EntityState != EntityState.Deleted);
        }
            
        public static async Task<List<TEntity>> LoadByUserIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId<Guid>
        {
            return await dbSet.AsNoTracking().Where(x => x.UserId == userId && x.EntityState!= EntityState.Deleted).ToListAsync();
        }
        
        public static async Task<List<Guid>> LoadIdsByUserIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid userId) where TEntity : class, IServerEntity, IHasUserId<Guid>
        {
            return await dbSet.AsNoTracking()
                .Where(x => x.UserId == userId && x.EntityState != EntityState.Deleted)
                .Select(x => x.Id)
                    .ToListAsync();
        }

        public static void AttachAndRemove<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class, IServerEntity
        {
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
         
         
    }
}