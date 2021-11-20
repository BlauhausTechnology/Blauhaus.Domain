using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public interface IDbContextBuilder<out TDbContext> where TDbContext : DbContext
    {
        TDbContext NewContext { get; }
    }
}