using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class SqliteInMemoryDbContextBuilder<TDbContext> : BaseDbContextBuilder<SqliteInMemoryDbContextBuilder<TDbContext>, TDbContext> where TDbContext : DbContext
    {
        private readonly LoggerFactory _loggerFactory;

        public SqliteInMemoryDbContextBuilder()
        {
            _loggerFactory = CreateLogger();
        }


        protected override DbContextOptions<TDbContext> GetOptions()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
 
            //This creates a SqliteConnectionwith that string
            var connection = new SqliteConnection(connectionString);
 
            //The connection MUST be opened here
            connection.Open();
            connection.EnableExtensions();

            //Now we have the EF Core commands to create SQLite options
            var options = new DbContextOptionsBuilder<TDbContext>();
            //options.EnableSensitiveDataLogging();
            //options.UseLoggerFactory(_loggerFactory);
            options.UseSqlite(connection);

            return options.Options;
        }
    }
}