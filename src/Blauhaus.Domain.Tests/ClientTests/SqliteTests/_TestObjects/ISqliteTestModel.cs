﻿using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public interface ISqliteTestModel : IClientEntity
    {
        string RootEntityName { get; }
    }
}