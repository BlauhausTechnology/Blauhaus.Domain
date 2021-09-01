using Blauhaus.Domain.Abstractions.Entities;
using System;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public interface ISqliteTestModel : IClientEntity<Guid>
    {
        string RootEntityName { get; }
    }
}