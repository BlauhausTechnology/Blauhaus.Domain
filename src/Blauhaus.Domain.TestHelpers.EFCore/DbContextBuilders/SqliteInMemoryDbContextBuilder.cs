using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class SqliteInMemoryDbContextBuilder<TDbContext> : BaseDbContextBuilder<SqliteInMemoryDbContextBuilder<TDbContext>, TDbContext> where TDbContext : DbContext
    {
        public SqliteInMemoryDbContextBuilder() 
            : base(GetOptions())
        {
        }

        private static DbContextOptions<TDbContext> GetOptions()
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
            options.EnableSensitiveDataLogging();
            options.UseLoggerFactory(LoggerFactory);
            options.UseSqlite(connection);

            return options.Options;
        }
    }
}