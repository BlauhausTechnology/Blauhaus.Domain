using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Entities;
using System;

namespace Blauhaus.Domain.Abstractions.Repositories
{
    public interface IDtoRepository<TDto> : IClientRepository<TDto, TDto>, IAsyncPublisher<TDto>
        where TDto : class, IClientEntity<Guid>
    {
        
    }
}