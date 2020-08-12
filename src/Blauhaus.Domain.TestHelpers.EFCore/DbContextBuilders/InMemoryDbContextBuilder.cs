using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class InMemoryDbContextBuilder<TDbContext> : BaseDbContextBuilder<InMemoryDbContextBuilder<TDbContext>, TDbContext> 
        where TDbContext : DbContext
    {
        private readonly LoggerFactory _loggerFactory;

        public InMemoryDbContextBuilder()
        {
            _loggerFactory = CreateLogger();
        }

        protected override DbContextOptions<TDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<TDbContext>()
                .UseLoggerFactory(_loggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        }
    }
}