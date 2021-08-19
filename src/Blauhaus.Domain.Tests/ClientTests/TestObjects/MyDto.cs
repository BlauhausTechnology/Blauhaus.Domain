﻿using Blauhaus.Domain.Abstractions.Entities;
using System;
using Blauhaus.Domain.Client.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class MyDto : ClientEntity<Guid>
    {
        public MyDto()
        {
            Id =  Guid.NewGuid();
        }
        public MyDto(Guid id)
        {
            Id = id;
        }

        public string Name { get; set; }
        
        public SyncState SyncState { get; set; }
    }
}