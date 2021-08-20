using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
    public interface ISyncDtoCache<TDto, in TId> : IDtoCache<TDto, TId> 
        where TDto : class, IClientEntity<TId>
    {
        Task<long> LoadLastModifiedAsync();
    }
}