using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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