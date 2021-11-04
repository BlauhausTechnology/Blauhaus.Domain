using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Client.DtoCaches
{
    public class InMemoryDtoCache<TDto, TId> : BasePublisher, IDtoCache<TDto, TId>
        where TDto : class, IHasId<TId> where TId : IEquatable<TId>
    {

        protected Dictionary<TId, TDto> CachedDtos = new Dictionary<TId, TDto>();

        public async Task HandleAsync(TDto dto)
        {
            CachedDtos[dto.Id] = dto;
            await UpdateSubscribersAsync(dto);
        }

        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

        public Task<TDto> GetOneAsync(TId id)
        {
            if (CachedDtos.TryGetValue(id, out var dto))
            {
                return Task.FromResult(dto);
            }
            throw new ErrorException(DomainErrors.NotFound<TDto>());
        }

        public Task<TDto?> TryGetOneAsync(TId id)
        {
            return Task.FromResult(CachedDtos
                .TryGetValue(id, out var dto)
                    ? dto
                    : null);
        }

        public Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<TDto>>(CachedDtos.Values.ToList());
        }

        public Task<IReadOnlyList<TDto>> GetWhereAsync(Func<TDto, bool> filter)
        {
            return Task.FromResult<IReadOnlyList<TDto>>(CachedDtos.Values.Where(filter).ToList());
        }

        public Task<IReadOnlyList<TId>> GetIdsWhereAsync(Func<TDto, bool> filter)
        {
            return Task.FromResult<IReadOnlyList<TId>>(
                CachedDtos.Values.Where(filter).Select(y => y.Id).ToList());
        }

        public Task DeleteOneAsync(TId id)
        {
            if (CachedDtos.TryGetValue(id, out _))
            {
                CachedDtos.Remove(id);
            }

            return Task.CompletedTask;

        }

        public Task DeleteAllAsync()
        {
            CachedDtos.Clear();
            return Task.CompletedTask;
        }
    }
}