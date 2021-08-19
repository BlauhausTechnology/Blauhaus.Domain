using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientActors.Actors;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Client.Sqlite.DtoCaches
{
    public class SyncDtoCache<TDto, TId, TCachedDtoEntity> : BaseActor, ISyncDtoCache<TDto, TId> 
        where TDto : class, IClientEntity<TId>
        where TCachedDtoEntity : ICachedDtoEntity<TCachedDtoEntity, TDto, TId>, new()
        where TId : IEquatable<TId>
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISqliteDatabaseService _sqliteDatabaseService;

        public SyncDtoCache(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService)
        {
            _analyticsService = analyticsService;
            _sqliteDatabaseService = sqliteDatabaseService;
        }

        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

        public async Task HandleAsync(TDto dto)
        {
            await _sqliteDatabaseService.AsyncConnection
                .InsertOrReplaceAsync(new TCachedDtoEntity().FromDto(dto));
            
            _analyticsService.Debug($"{typeof(TDto).Name} saved as {typeof(TCachedDtoEntity).Name}");

            await UpdateSubscribersAsync(dto);
        }

        public async Task<TDto> GetOneAsync(TId id)
        {
            var entity = await _sqliteDatabaseService.AsyncConnection
                .Table<TCachedDtoEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));

            if (entity == null)
            {
                throw new ErrorException(DomainErrors.NotFound<TDto>());
            }

            return entity.ToDto();
        }

        public async Task<TDto?> TryGetOneAsync(TId id)
        {
            var entity = await _sqliteDatabaseService.AsyncConnection.Table<TCachedDtoEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            return entity == null ? null : entity.ToDto();
        }

        public async Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            var entities = await _sqliteDatabaseService.AsyncConnection
                .Table<TCachedDtoEntity>().ToListAsync();

            return entities.Select(x => x.ToDto()).ToArray();
        }

        public async Task<IReadOnlyList<TDto>> GetWhereAsync(Func<TDto, bool> search)
        {
            var entities = await _sqliteDatabaseService.AsyncConnection
                .Table<TCachedDtoEntity>().ToListAsync();
            
            return entities.Select(x => x.ToDto())
                .Where(search.Invoke).ToArray();
        }

        public Task<long> LoadLastModifiedAsync()
        {
            return _sqliteDatabaseService.AsyncConnection
                .ExecuteScalarAsync<long>($"SELECT ModifiedAtTicks FROM {typeof(TCachedDtoEntity).Name} ORDER BY ModifiedAtTicks DESC LIMIT 1");
        }
    }
}