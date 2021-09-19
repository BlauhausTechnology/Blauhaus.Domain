using System;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Sync
{

    public class SyncDtoCacheMockBuilder<TDto, TId> : BaseSyncDtoCacheMockBuilder<SyncDtoCacheMockBuilder<TDto, TId>, ISyncDtoCache<TDto, TId>, TDto, TId>
        where TDto : class, IClientEntity<TId> where TId : IEquatable<TId>
    {

    }

    public abstract class BaseSyncDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> : BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId>
        where TBuilder : BaseSyncDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> 
        where TMock : class, ISyncDtoCache<TDto, TId>
        where TDto : class, IClientEntity<TId>
        where TId : IEquatable<TId>
    {

        public TBuilder Where_LoadLastModifiedTicksAsync_returns(long value)
        {
            Mock.Setup(x => x.LoadLastModifiedTicksAsync())
                .ReturnsAsync(value);
            return (TBuilder)this;
        }
    }
}