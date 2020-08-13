using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class SqliteInMemoryDbContextBuilder<TDbContext> : BaseDbContextBuilder<SqliteInMemoryDbContextBuilder<TDbContext>, TDbContext> 
        where TDbContext : DbContext
    {
        public SqliteInMemoryDbContextBuilder() : base(true)
        {
        }
    }
}