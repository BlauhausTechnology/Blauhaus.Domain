using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public interface ISqliteTestDto
    {
        public string RootEntityName { get; }
        public Guid Id { get; }
        public EntityState EntityState { get; }
        public long ModifiedAtTicks { get; }
    }
}