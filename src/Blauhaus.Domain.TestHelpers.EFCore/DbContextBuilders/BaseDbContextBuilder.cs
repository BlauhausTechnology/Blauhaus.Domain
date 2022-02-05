using System;
using Blauhaus.TestHelpers.Builders.Base;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public abstract class BaseDbContextBuilder<TBuilder, TDbContext> : BaseBuilder<TBuilder, TDbContext>, IDbContextBuilder<TDbContext>
        where TDbContext : DbContext 
        where TBuilder : BaseDbContextBuilder<TBuilder, TDbContext>
    {
        private readonly DbContextOptions<TDbContext> _options;

        protected BaseDbContextBuilder(bool useSqlLite)
        {
            _options = useSqlLite ? GetSqLiteDbContextOptions() : GetInMemoryDbContextOptions();
        }
        
        public TDbContext NewContext => ((TDbContext) Activator.CreateInstance(typeof(TDbContext), _options)!)!;

        protected override TDbContext Construct()
        {
            var context = (TDbContext) Activator.CreateInstance(typeof(TDbContext), _options)!;
            context.Database.EnsureCreated();
            return context;
        }

        private readonly ILoggerFactory _myLoggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); 
            });

        private DbContextOptions<TDbContext> GetInMemoryDbContextOptions()
        {
            return new DbContextOptionsBuilder<TDbContext>()
                .UseLoggerFactory(_myLoggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        }

        private DbContextOptions<TDbContext> GetSqLiteDbContextOptions()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "file::memory:?cache=shared" };
            var connectionString = connectionStringBuilder.ToString();
 
            //This creates a SqliteConnectionwith that string
            var connection = new SqliteConnection(connectionString);
 
            //The connection MUST be opened here
            connection.Open();
            connection.EnableExtensions();

            //Now we have the EF Core commands to create SQLite options
            var options = new DbContextOptionsBuilder<TDbContext>();
            options.EnableSensitiveDataLogging();
            options.UseLoggerFactory(_myLoggerFactory);
            options.UseSqlite(connection);

            return options.Options;
        }
    }
}