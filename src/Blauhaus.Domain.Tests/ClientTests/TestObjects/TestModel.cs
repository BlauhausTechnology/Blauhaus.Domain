using System;
using Blauhaus.Domain.Client.CommandHandlers.Entities;
using Blauhaus.Domain.Client.Entities;

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
    }
}