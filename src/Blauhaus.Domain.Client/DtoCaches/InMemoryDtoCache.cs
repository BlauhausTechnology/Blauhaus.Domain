using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.ClientActors.Actors;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Client.DtoCaches
{
    public class InMemoryDtoCache<TDto, TId> : BaseActor, IDtoCache<TDto, TId>
        where TDto : class, IHasId<TId> where TId : IEquatable<TId>
    {

        protected Dictionary<TId, TDto> CachedDtos = new Dictionary<TId, TDto>();

        public Task HandleAsync(TDto dto)
        {
            return InvokeAsync(async () =>
            {
                CachedDtos[dto.Id] = dto;
                await UpdateSubscribersAsync(dto);
            });
        }

        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            return InvokeAsync(() => AddSubscriber(handler, filter));
        }

        public Task<TDto> GetOneAsync(TId id)
        {
            return InvokeAsync(() =>
            {
                if (CachedDtos.TryGetValue(id, out var dto))
                {
                    return dto;
                }
                throw new ErrorException(DomainErrors.NotFound<TDto>());
            });
        }

        public Task<TDto?> TryGetOneAsync(TId id)
        {
            return InvokeAsync(() =>
                CachedDtos.TryGetValue(id, out var dto)
                    ? dto
                    : null);
        }

        public Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return InvokeAsync<IReadOnlyList<TDto>>(() =>
                CachedDtos.Values.ToList());
        }
         
        public Task DeleteOneAsync(TId id)
        {
            return InvokeAsync(() =>
            {
                if (CachedDtos.TryGetValue(id, out _))
                {
                    CachedDtos.Remove(id);
                }
            });

        }

        public Task DeleteAllAsync()
        {
            return InvokeAsync(() =>
            {
                CachedDtos.Clear();
            });
        }
    }
}