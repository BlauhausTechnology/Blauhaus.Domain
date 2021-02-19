using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public class InMemoryDbContextBuilder<TDbContext> : BaseDbContextBuilder<InMemoryDbContextBuilder<TDbContext>, TDbContext> 
        where TDbContext : DbContext
    {
        public InMemoryDbContextBuilder() : base(false)
        {
        }
    }
}