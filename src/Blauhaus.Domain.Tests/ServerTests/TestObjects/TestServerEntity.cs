using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Tests.ServerTests.TestObjects
{
    public class TestServerEntity : IServerEntity
    {
        public TestServerEntity(Guid id, EntityState entityState, DateTime createdAt, DateTime modifiedAt)
        {
            Id = id;
            EntityState = entityState;
            CreatedAt = createdAt;
            ModifiedAt = modifiedAt;
        }

        public Guid Id { get; }
        public EntityState EntityState { get; }
        public long ModifiedAtTicks => ModifiedAt.Ticks;
        public DateTime CreatedAt { get; }
        public DateTime ModifiedAt { get; set; }


        public static IQueryable<TestServerEntity> GenerateList(int numberToGenerate)
        {
            var entityList = new List<TestServerEntity>();

            for (var i = 0; i < numberToGenerate; i++)
            {
                var modifiedAt = DateTime.UtcNow.AddDays(-i);
                var createdAt = modifiedAt.AddDays(-10);
                
                var id = Guid.NewGuid();
                entityList.Add(new TestServerEntity(id, EntityState.Active, createdAt, modifiedAt));
            }

            return entityList.OrderByDescending(x => x.ModifiedAt).ToList().AsQueryable();
        }
    }
}