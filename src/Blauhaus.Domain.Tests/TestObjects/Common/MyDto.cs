using System;
using Blauhaus.Domain.Client.Sqlite.Entities;

namespace Blauhaus.Domain.Tests.TestObjects.Common
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
        
    }
}