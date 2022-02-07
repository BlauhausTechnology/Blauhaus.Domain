using System;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public interface IDbContextBuilder<out TDbContext> : IDisposable
        where TDbContext : DbContext
    {
        TDbContext NewContext { get; }
    }
}