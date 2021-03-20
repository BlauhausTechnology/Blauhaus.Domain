using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
   public static class DbSetExtensions
    {

        public static TEntity? LoadById<TEntity>(this DbSet<TEntity> dbSet, Guid id) where TEntity : class, IServerEntity
        {
            return dbSet.AsNoTracking().FirstOrDefault(x =>
                x.Id == id && 
                x.EntityState != EntityState.Deleted);
        }
        public static async Task<TEntity?> LoadByIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid id) where TEntity : class, IServerEntity
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x => 
                x.Id == id && 
                x.EntityState != EntityState.Deleted);
        }


        
        public static TEntity? LoadOneBy<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity
        {
            return dbSet.AsNoTracking().FirstOrDefault(x =>
                filter.Invoke(x) &&
                x.EntityState != EntityState.Deleted);
        }
        public static async Task<TEntity?> LoadOneByAsync<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x =>
                filter.Invoke(x) &&
                x.EntityState != EntityState.Deleted);
        }
        
        
        public static IReadOnlyList<TEntity> LoadWhere<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity
        {
            return dbSet.AsNoTracking().Where(x =>
                filter.Invoke(x) &&
                x.EntityState != EntityState.Deleted)
                .ToList();
        }
        public static async Task<IReadOnlyList<TEntity>> LoadWhereAsync<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity
        {
            return await dbSet.AsNoTracking().Where(x =>
                    filter.Invoke(x) &&
                    x.EntityState != EntityState.Deleted)
                .ToListAsync();
        }
                
        
        public static void AttachAndRemove<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class, IServerEntity
        {
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
         
         
    }
}