using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.DtoCaches
{
    public abstract class BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId>: BaseDtoLoaderMockBuilder<TBuilder, TMock, TDto, TId>
        where TBuilder: BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId>
        where TMock : class, IDtoCache<TDto, TId>
        where TId : IEquatable<TId>
        where TDto : class, IHasId<TId>
    {
        
       
    }
}