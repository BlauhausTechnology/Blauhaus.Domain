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
    public abstract class BaseDbContextBuilder<TBuilder, TContext> : BaseBuilder<TBuilder, TContext>
        where TContext : DbContext 
        where TBuilder : BaseDbContextBuilder<TBuilder, TContext>
    {
        private readonly DbContextOptions<TContext> _options;

        protected static readonly ILoggerFactory LoggerFactory;

        protected BaseDbContextBuilder(DbContextOptions<TContext> options)
        {
            _options = options;
            LoggerFactory 
                = new LoggerFactory(new[] 
                {
                    new ConsoleLoggerProvider((_, __) => true, true)
                });
        }


        protected override TContext Construct()
        {
            return (TContext) Activator.CreateInstance(typeof(TContext), _options);
        }

       
    }
}