using Microsoft.EntityFrameworkCore;

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