using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches
{

    public class SyncDtoCacheMockBuilder<TDto, TEntity, TId> : BaseSyncDtoCacheMockBuilder<SyncDtoCacheMockBuilder<TDto, TEntity, TId>, ISyncDtoCache<TDto, TEntity, TId>, TDto, TEntity, TId>
        where TDto : class, IClientEntity<TId> 
        where TEntity : class, ISyncClientEntity<TId>
    {

    }

    public abstract class BaseSyncDtoCacheMockBuilder<TBuilder, TMock, TDto, TEntity, TId> : BaseDtoCacheMockBuilder<TBuilder, TMock, TDto, TEntity, TId>
        where TBuilder : BaseSyncDtoCacheMockBuilder<TBuilder, TMock, TDto, TEntity, TId> 
        where TMock : class, ISyncDtoCache<TDto, TEntity, TId>
        where TDto : class, IClientEntity<TId>
        where TEntity : class, ISyncClientEntity<TId>
    {
        
    }
}