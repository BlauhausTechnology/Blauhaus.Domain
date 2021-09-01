using System;
using System.Security.Cryptography;
using Blauhaus.Domain.Server.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blauhaus.Domain.Server.EFCore.DbMappings
{
    //public abstract class BaseDbMapping<TEntity> : BaseDbMapping<TEntity, Guid> where TEntity : BaseServerEntity<Guid>
    //{
    //    protected BaseDbMapping(ModelBuilder modelBuilder) : base(modelBuilder)
    //    {
    //    }
    //}


    public abstract class BaseDbMapping<TEntity, TId> : IEntityTypeConfiguration<TEntity> where TEntity : BaseServerEntity<TId>
    {
        protected BaseDbMapping(ModelBuilder modelBuilder)
        {
            var tableName = typeof(TEntity).Name;
            var index = tableName.IndexOf("Entity", StringComparison.InvariantCultureIgnoreCase);
            if (index > 0)
            {
                tableName = tableName.Remove(index, 6);
            }

            modelBuilder
                .Entity<TEntity>()
                .ToTable(tableName);
        }

        public void Configure(EntityTypeBuilder<TEntity> entityTypeBuilder)
        {
            entityTypeBuilder.HasKey(x => x.Id);
            ConfigureEntity(entityTypeBuilder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> entity);
    }
}