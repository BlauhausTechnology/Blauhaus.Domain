using System;
using Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders;
using Blauhaus.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.TestHelpers.EFCore.Extensions
{
    public static class MockContainerExtensions
    {
        public static Func<SqliteInMemoryDbContextBuilder<TDbContext>> AddSqliteDbContext<TDbContext>(this MockContainer mocks) 
            where TDbContext : DbContext
        {
            return mocks.AddMock<SqliteInMemoryDbContextBuilder<TDbContext>, TDbContext>();
        }

        public static Func<InMemoryDbContextBuilder<TDbContext>> AdInMemoryDbContext<TDbContext>(this MockContainer mocks) 
            where TDbContext : DbContext
        {
            return mocks.AddMock<InMemoryDbContextBuilder<TDbContext>, TDbContext>();
        }
    }
}