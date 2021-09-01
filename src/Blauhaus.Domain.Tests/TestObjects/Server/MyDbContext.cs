using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Tests.TestObjects.Server
{
    public class MyDbContext : DbContext
    {
        public DbSet<MySyncServerEntity> MySyncServerEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}