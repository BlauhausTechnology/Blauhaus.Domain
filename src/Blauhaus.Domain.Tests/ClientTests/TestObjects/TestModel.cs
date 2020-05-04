using System;
using System.Collections.Generic;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestModel : IClientEntity
    {
        public TestModel(Guid id, EntityState entityState, long modifiedAtTicks, string name)
        {
            Id = id;
            EntityState = entityState;
            ModifiedAtTicks = modifiedAtTicks;
            Name = name;
        }

        public Guid Id { get; }
        public EntityState EntityState { get; }
        public long ModifiedAtTicks { get; }

        public string Name { get; }

        public static List<TestModel> GenerateList(int numberToGenerate)
        {
            var models = new List<TestModel>();
            for (var i = 0; i < numberToGenerate; i++)
            {
                models.Add(new TestModel(Guid.NewGuid(), EntityState.Active, DateTime.UtcNow.AddDays(-i).Ticks, i.ToString()));
            }
            return models;
        }
    }
}