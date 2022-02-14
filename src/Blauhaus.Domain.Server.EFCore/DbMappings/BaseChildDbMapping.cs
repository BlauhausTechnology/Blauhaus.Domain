using Blauhaus.Domain.Server.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;

namespace Blauhaus.Domain.Server.EFCore.DbMappings
{
    public abstract class BaseChildDbMapping<TEntity, TId> : IEntityTypeConfiguration<TEntity> where TEntity : BaseChildEntity<TId>
    {
        protected BaseChildDbMapping(ModelBuilder modelBuilder)
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


    public abstract class BaseChildDbMapping<TEntity> : BaseChildDbMapping<TEntity, Guid> where TEntity : BaseChildEntity
    {
        protected BaseChildDbMapping(ModelBuilder modelBuilder) : base(modelBuilder)
        {
        }
    }
}