using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.Extensions
{
    public static class DbContextExtensions
    {
        public static T Seed<TDbContext, T>(this TDbContext dbContext, T entity) 
            where TDbContext : DbContext
        {
            dbContext.Add(entity);
            return entity;
        }
    }
}