using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Server.Entities;
using Microsoft.EntityFrameworkCore;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
   public static class DbSetExtensions
    {

        public static TEntity? LoadById<TEntity>(this DbSet<TEntity> dbSet, Guid id) where TEntity : class, IServerEntity<Guid>
        {
            return dbSet.AsNoTracking().FirstOrDefault(x =>
                x.Id == id && 
                x.EntityState != EntityState.Deleted);
        }
        public static async Task<TEntity?> LoadByIdAsync<TEntity>(this DbSet<TEntity> dbSet, Guid id) where TEntity : class, IServerEntity<Guid>
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x => 
                x.Id == id && 
                x.EntityState != EntityState.Deleted);
        }


        
        public static TEntity? LoadOneBy<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity<Guid>
        {
            return dbSet.AsNoTracking().FirstOrDefault(x =>
                filter.Invoke(x) &&
                x.EntityState != EntityState.Deleted);
        }
        public static async Task<TEntity?> LoadOneByAsync<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity<Guid>
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(x =>
                filter.Invoke(x) &&
                x.EntityState != EntityState.Deleted);
        }
        
        
        public static IReadOnlyList<TEntity> LoadWhere<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity<Guid>
        {
            return dbSet.AsNoTracking().Where(x =>
                filter.Invoke(x) &&
                x.EntityState != EntityState.Deleted)
                .ToList();
        }
        public static async Task<IReadOnlyList<TEntity>> LoadWhereAsync<TEntity>(this DbSet<TEntity> dbSet, Func<TEntity, bool> filter) where TEntity : class, IServerEntity<Guid>
        {
            return await dbSet.AsNoTracking().Where(x =>
                    filter.Invoke(x) &&
                    x.EntityState != EntityState.Deleted)
                .ToListAsync();
        }
                
        
        public static void AttachAndRemove<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class, IServerEntity<Guid>
        {
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }

        public static void Delete<TEntity>(this DbSet<TEntity> dbSet, DateTime now, TEntity entity) 
            where TEntity : BaseServerEntity<Guid>
        {
            dbSet.Attach(entity);
            entity.Delete(now);
        }

        public static void DeleteWhere<TEntity>(this DbSet<TEntity> dbSet, DateTime now, Func<TEntity,bool> filter) 
            where TEntity : BaseServerEntity<Guid>
        {
            foreach (var entity in dbSet.Where(filter))
            {
                dbSet.Attach(entity);
                entity.Delete(now);
            }
        }
         
         
    }
}