using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestDto : ISqliteTestDto
    {
        public SqliteTestDto(Guid id, EntityState entityState, long modifiedAtTicks, string rootEntityName)
        {
            Id = id;
            EntityState = entityState;
            ModifiedAtTicks = modifiedAtTicks;
            RootEntityName = rootEntityName;
        }

        public string RootEntityName { get; }
        public Guid Id { get; }
        public EntityState EntityState { get; }
        public long ModifiedAtTicks { get; }
    }
}