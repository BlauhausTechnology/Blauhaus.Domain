using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.MockBuilders;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.DtoCaches
{

    public class DtoLoaderMockBuilder<TDto, TId> : BaseDtoLoaderMockBuilder<DtoLoaderMockBuilder<TDto, TId>, IDtoLoader<TDto, TId>, TDto, TId> 
        where TId : IEquatable<TId>
        where TDto : class, IHasId<TId>
    {

    }

    public abstract class BaseDtoLoaderMockBuilder<TBuilder, TMock, TDto, TId>: BaseAsyncPublisherMockBuilder<TBuilder, TMock, TDto>
        where TBuilder: BaseDtoLoaderMockBuilder<TBuilder, TMock, TDto, TId>
        where TMock : class, IDtoLoader<TDto, TId>
        where TId : IEquatable<TId>
        where TDto : class, IHasId<TId>
    {
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