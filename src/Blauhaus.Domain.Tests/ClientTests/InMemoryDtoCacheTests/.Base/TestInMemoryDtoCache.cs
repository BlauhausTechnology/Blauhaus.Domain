using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.DtoCaches;
using Blauhaus.Domain.Tests.TestObjects.Common;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base
{
    public class TestInMemoryDtoCache: InMemoryDtoCache<MyDto, Guid>
    {
        public Dictionary<Guid, MyDto> Cache => CachedDtos;
       
    }
}