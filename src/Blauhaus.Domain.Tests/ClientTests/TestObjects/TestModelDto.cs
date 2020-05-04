using System;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestModelDto
    {
        public Guid Id { get; set; }
        public EntityState EntityState { get;  set;}
        public long ModifiedAtTicks { get;  set;}
        public string Name { get;  set;}
    }
}