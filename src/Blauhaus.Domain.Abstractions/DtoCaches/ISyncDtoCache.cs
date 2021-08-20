using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface ISyncDtoCache<TDto, TEntity, in TId> : IDtoCache<TDto, TEntity, TId> 
        where TDto : class, IClientEntity<TId>
        where TEntity : class, ISyncClientEntity<TId>
    {
        Task<long> LoadLastModifiedAsync();
    }
}