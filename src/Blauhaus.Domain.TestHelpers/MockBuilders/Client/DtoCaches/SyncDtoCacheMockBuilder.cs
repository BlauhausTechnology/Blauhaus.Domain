using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches
{

    public class SyncDtoCacheMockBuilder<TDto, TId> : BaseSyncDtoCacheMockBuilder<SyncDtoCacheMockBuilder<TDto, TId>, ISyncDtoCache<TDto, TId>, TDto, TId>
        where TDto : class, IClientEntity<TId>
    {

    }

    public abstract class BaseSyncDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> : BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TId>
        where TBuilder : BaseSyncDtoCacheMockBuilder<TBuilder, TMock, TDto, TId> 
        where TMock : class, ISyncDtoCache<TDto, TId>
        where TDto : class, IClientEntity<TId>
    {
        
    }
}