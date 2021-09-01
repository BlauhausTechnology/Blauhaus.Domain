﻿using Blauhaus.Common.Abstractions;
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

    public abstract class BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> : BaseAsyncPublisherMockBuilder<TBuilder, TMock, TDto>
        where TBuilder : BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> 
        where TMock : class, IDtoCache<TDto, TId>
        where TDto : class, IHasId<TId>
        where TId : IEquatable<TId>
    {
        public void VerifySaveAsync(TDto dto, int times = 1)
        {
            Mock.Verify(x => x.HandleAsync(dto), Times.Exactly(times));
        }
        
        public TBuilder Where_TryGetOneAsync_returns(TDto? dto)
        {
            Mock.Setup(x => x.TryGetOneAsync(It.IsAny<TId>())).ReturnsAsync(dto);
            return (TBuilder) this;
        }
        public TBuilder Where_TryGetOneAsync_returns(Func<TDto?> dto)
        {
            Mock.Setup(x => x.TryGetOneAsync(It.IsAny<TId>())).ReturnsAsync(dto);
            return (TBuilder) this;
        }
        public TBuilder Where_TryGetOneAsync_returns(TDto? dto, TId id)
        {
            Mock.Setup(x => x.TryGetOneAsync(id)).ReturnsAsync(dto);
            return (TBuilder) this;
        }

        public TBuilder Where_GetOneAsync_returns(TDto dto)
        {
            Mock.Setup(x => x.GetOneAsync(It.IsAny<TId>())).ReturnsAsync(dto);
            return (TBuilder) this;
        }
        public TBuilder Where_GetOneAsync_returns(Func<TDto> dto)
        {
            Mock.Setup(x => x.GetOneAsync(It.IsAny<TId>())).ReturnsAsync(dto);
            return (TBuilder) this;
        }
        public TBuilder Where_GetOneAsync_returns(TDto dto, TId id)
        {
            Mock.Setup(x => x.GetOneAsync(id)).ReturnsAsync(dto);
            return (TBuilder) this;
        }
        
        public TBuilder Where_GetAllAsync_returns(TDto dto)
        {
            Mock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TDto>{dto});
            return (TBuilder) this;
        } 
        public TBuilder Where_GetAllAsync_returns(Func<TDto> dto)
        {
            Mock.Setup(x => x.GetAllAsync()).ReturnsAsync(() => new List<TDto>{dto.Invoke()});
            return (TBuilder) this;
        }        
        public TBuilder Where_GetAllAsync_returns(Func<IEnumerable<TDto>> dto)
        {
            Mock.Setup(x => x.GetAllAsync()).ReturnsAsync(() => dto.Invoke().ToArray());
            return (TBuilder) this;
        } 
        public TBuilder Where_GetAllAsync_returns(IEnumerable<TDto> dtos)
        {
            Mock.Setup(x => x.GetAllAsync()).ReturnsAsync(dtos.ToList());
            return (TBuilder) this;
        } 
  
    }
}