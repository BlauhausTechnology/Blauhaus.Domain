using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class InMemoryDbContextBuilder<TDbContext> : BaseDbContextBuilder<InMemoryDbContextBuilder<TDbContext>, TDbContext> 
        where TDbContext : DbContext
    {  

        protected override DbContextOptions<TDbContext> GetOptions(LoggerFactory loggerFactory)
        {
            return new DbContextOptionsBuilder<TDbContext>()
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        }
    }
}