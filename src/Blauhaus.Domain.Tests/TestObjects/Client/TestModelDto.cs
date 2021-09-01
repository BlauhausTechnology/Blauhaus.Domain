using System;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Tests.TestObjects.Client
{
    public class TestModelDto
    {
        public Guid Id { get; set; }
        public EntityState EntityState { get;  set;}
        public long ModifiedAtTicks { get;  set;}
        public string Name { get;  set;}
    }
}