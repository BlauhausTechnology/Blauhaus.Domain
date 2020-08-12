using System;
using Blauhaus.TestHelpers.Builders._Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Blauhaus.Domain.TestHelpers.EFCore.DbContextBuilders
{
    public abstract class BaseDbContextBuilder<TBuilder, TDbContext> : BaseBuilder<TBuilder, TDbContext>
        where TDbContext : DbContext 
        where TBuilder : BaseDbContextBuilder<TBuilder, TDbContext>
    {
        private readonly DbContextOptions<TDbContext> _options;
        private readonly LoggerFactory _loggerFactory;
         
        protected BaseDbContextBuilder()
        {
            _loggerFactory = new LoggerFactory(new[] 
            {
                new ConsoleLoggerProvider((_, __) => true, true)
            });

        }


        protected override TDbContext Construct()
        {
            return (TDbContext) Activator.CreateInstance(typeof(TDbContext), GetOptions(_loggerFactory));
        }

        protected abstract DbContextOptions<TDbContext> GetOptions(LoggerFactory loggerFactory);
    }
}