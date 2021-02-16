using System;
using Blauhaus.Common.Utils.Contracts;
using Blauhaus.Domain.Abstractions.Entities;
using NUnit.Framework;

namespace Blauhaus.Domain.TestHelpers.Extensions
{ 
    public static class ServerEntityExtensions
    {
        public static TEntity VerifyModifiedServerEntity<TEntity>(this TEntity entity, DateTime createdAt, DateTime modifiedAt) 
            where TEntity : IServerEntity
        {
            Assert.That(entity, Is.Not.Null);
            
            Assert.That(entity!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(entity.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(entity.ModifiedAt, Is.EqualTo(modifiedAt));
            Assert.That(entity.EntityState, Is.EqualTo(EntityState.Active));
            
            return entity;
        }
        
        public static TEntity VerifyNewServerEntity<TEntity>(this TEntity entity, DateTime runTime) 
            where TEntity : IServerEntity
        {
            Assert.That(entity, Is.Not.Null);
            
            Assert.That(entity!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(entity.CreatedAt, Is.EqualTo(runTime));
            Assert.That(entity.ModifiedAt, Is.EqualTo(runTime));
            Assert.That(entity.EntityState, Is.EqualTo(EntityState.Active));
            
            return entity;
        }
        
        public static TEntity VerifyNewServerUserEntity<TEntity>(this TEntity entity, DateTime createdAt, Guid userId) 
            where TEntity : IServerEntity, IHasUserId<Guid>
        {
            entity = entity.VerifyNewServerEntity(createdAt);
            Assert.That(entity.UserId, Is.EqualTo(userId));
            
            return entity;
        }
        
        public static TEntity VerifyModifiedServerUserEntity<TEntity>(this TEntity entity, DateTime createdAt, DateTime modifiedAt, Guid userId) 
            where TEntity : IServerEntity, IHasUserId<Guid>
        {
            entity = entity.VerifyModifiedServerEntity(createdAt, modifiedAt);
            Assert.That(entity.UserId, Is.EqualTo(userId));
            
            return entity;
        }
         
    }
}