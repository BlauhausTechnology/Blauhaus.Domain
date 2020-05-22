using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Repositories
{
    public interface IClientRepository<TModel, in TDto> 
        where TModel : class, IClientEntity
    {
        Task<TModel?> LoadByIdAsync(Guid id);
        Task<TModel> SaveDtoAsync(TDto dto); 
    }
}