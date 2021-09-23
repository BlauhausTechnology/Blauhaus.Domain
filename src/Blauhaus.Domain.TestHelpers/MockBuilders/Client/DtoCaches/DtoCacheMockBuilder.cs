using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches
{
    public class DtoCacheMockBuilder<TDto, TId> : BaseDtoCacheMockBuilder<DtoCacheMockBuilder<TDto, TId>, IDtoCache<TDto, TId>, TDto, TId> 
        where TDto : class, IHasId<TId> where TId : IEquatable<TId>
    {

    }

    public abstract class BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> : Common.DtoCaches.BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId>
        where TBuilder : BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> 
        where TMock : class, IDtoCache<TDto, TId>
        where TDto : class, IHasId<TId>
        where TId : IEquatable<TId>
    {
        public void VerifySaveAsync(TDto dto, int times = 1)
        {
            Mock.Verify(x => x.HandleAsync(dto), Times.Exactly(times));
        }
        
  
    }
}