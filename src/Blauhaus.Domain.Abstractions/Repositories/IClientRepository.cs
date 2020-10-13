using System;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.Repositories
{

    public interface IClientRepository<TModel>
        where TModel : class, IClientEntity
    {
        Task<TModel?> LoadByIdAsync(Guid id); 
    }

    public interface IClientRepository<TModel, in TDto> : IClientRepository<TModel>
        where TModel : class, IClientEntity
    {
        Task<TModel> SaveDtoAsync(TDto dto); 
    }
}