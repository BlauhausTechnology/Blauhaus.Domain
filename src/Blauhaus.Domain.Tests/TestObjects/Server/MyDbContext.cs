using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Tests.TestObjects.Server
{
    public class MyDbContext : DbContext
    {

        public MyDbContext() { }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }



        public DbSet<MyServerEntity> MyEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}