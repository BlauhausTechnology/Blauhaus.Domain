using System;
using Blauhaus.TestHelpers.Builders._Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public abstract class BaseDbContextBuilder<TBuilder, TDbContext> : BaseBuilder<TBuilder, TDbContext>
        where TDbContext : DbContext 
        where TBuilder : BaseDbContextBuilder<TBuilder, TDbContext>
    {
          

        protected override TDbContext Construct()
        {
            return (TDbContext) Activator.CreateInstance(typeof(TDbContext), GetOptions());
        }

        protected abstract DbContextOptions<TDbContext> GetOptions();

        protected LoggerFactory CreateLogger() 
        {
            return new LoggerFactory (new [] {
                new ConsoleLoggerProvider ((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name &&
                    level == LogLevel.Information, true)
            });
        }
    }
}