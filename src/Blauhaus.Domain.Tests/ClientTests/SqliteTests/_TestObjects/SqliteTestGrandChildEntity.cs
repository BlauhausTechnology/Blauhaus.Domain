﻿using System;
using Blauhaus.Domain.Client.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public class SqliteTestGrandChildEntity: BaseSyncClientEntity
    {
        public Guid ChildEntityId { get; set; }
        public string GrandchildName { get; set; }
    }
}