using System;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class InMemoryDbContextProvider<TDbContext> : BaseDbContextBuilder<InMemoryDbContextProvider<TDbContext>, TDbContext> 
        where TDbContext : DbContext
    {
        public InMemoryDbContextProvider() 
            : base(GetOptions())
        {
        }

        private static DbContextOptions<TDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<TDbContext>()
                .UseLoggerFactory(LoggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        }
    }
}