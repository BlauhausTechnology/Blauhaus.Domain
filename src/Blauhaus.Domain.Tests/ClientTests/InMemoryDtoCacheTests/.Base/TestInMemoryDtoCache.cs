using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.DtoCaches;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests._.Base
{
    public class TestInMemoryDtoCache: InMemoryDtoCache<MyDto, Guid>
    {
        public Dictionary<Guid, MyDto> Cache => CachedDtos;
       
    }
}