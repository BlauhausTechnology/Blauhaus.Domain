using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Server.Entities;

namespace Blauhaus.Domain.Tests.TestObjects.Server
{
    public class MyServerEntity : BaseServerEntity
    {

        internal MyServerEntity()
        {
        }
         
        public MyServerEntity(DateTime createdAt, Guid id, EntityState entityState, string name) 
            : base(createdAt, id, entityState)
        {
            Name = name;
        }

        public string Name { get; private set; } = string.Empty;

    }
}