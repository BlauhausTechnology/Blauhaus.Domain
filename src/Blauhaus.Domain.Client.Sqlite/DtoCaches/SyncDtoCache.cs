using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientActors.Actors;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Errors;
using Newtonsoft.Json;

namespace Blauhaus.Domain.Client.Sqlite.DtoCaches
{
    public class SyncDtoCache<TDto, TEntity, TId> : BaseActor, ISyncDtoCache<TDto, TId> 
        where TDto : class, IClientEntity<TId>
        where TEntity : SyncClientEntity<TId>, new()
        where TId : IEquatable<TId>
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISqliteDatabaseService _sqliteDatabaseService;
        private readonly string _lastModifiedQueryStart;
        private readonly string _lastModifiedQueryEnd;

        public SyncDtoCache(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService)
        {
            _analyticsService = analyticsService;
            _sqliteDatabaseService = sqliteDatabaseService;

            _lastModifiedQueryStart = $"SELECT {nameof(ISyncClientEntity.ModifiedAtTicks)} " +
                                 $"FROM {typeof(TEntity).Name} " +
                                 $"WHERE {nameof(ISyncClientEntity.SyncState)} == {(int)SyncState.InSync} ";

            _lastModifiedQueryEnd = $"ORDER BY {nameof(ISyncClientEntity.ModifiedAtTicks)} DESC LIMIT 1";

        }
        public Task<long> LoadLastModifiedTicksAsync(IKeyValueProvider? settingsProvider)
        {
            return InvokeLockedAsync(async () =>
            {
                var lastModifiedQuery = new StringBuilder();
                    
                lastModifiedQuery.Append(_lastModifiedQueryStart);

                if (settingsProvider != null)
                {
                    GetAdditionalFilterClause(settingsProvider);
                }
                    
                lastModifiedQuery.Append(_lastModifiedQueryEnd);

                return await _sqliteDatabaseService.AsyncConnection.ExecuteScalarAsync<long>(lastModifiedQuery.ToString());
            });
        }

        public Task SaveSyncedDtosAsync(DtoBatch<TDto, TId> dtoBatch)
        {
            return InvokeAsync(async () =>
            {
                //todo cache BatchLastModifiedTicks?

                foreach (var dto in dtoBatch.Dtos)
                {
                    var entity = await PopulateEntityAsync(dto);
                    entity.SyncState = SyncState.InSync;

                    await _sqliteDatabaseService.AsyncConnection
                        .InsertOrReplaceAsync(entity);

                    _analyticsService.Debug($"{typeof(TDto).Name} saved as {typeof(TEntity).Name}");

                    await UpdateSubscribersAsync(dto);
                }
            });
        }

        protected virtual string GetAdditionalFilterClause(IKeyValueProvider command)
        {
            return string.Empty;
        }


        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

        public Task HandleAsync(TDto dto)
        {
            return InvokeAsync(async () =>
            {
                await _sqliteDatabaseService.AsyncConnection
                    .InsertOrReplaceAsync(await PopulateEntityAsync(dto));

                _analyticsService.Debug($"{typeof(TDto).Name} saved as {typeof(TEntity).Name}");

                await UpdateSubscribersAsync(dto);
            });
        }

        public Task<TDto> GetOneAsync(TId id)
        {
            return InvokeAsync(async () =>
            {
                var dto = await LoadOneAsync(id);

                if (dto == null)
                {
                    throw new ErrorException(DomainErrors.NotFound<TDto>());
                }

                return dto;
            });
        }

        public Task<TDto?> TryGetOneAsync(TId id)
        {
            return InvokeAsync(async ()=> await LoadOneAsync(id));
        }

        public Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return InvokeAsync(async () => await LoadManyAsync(x => true));
        }
         
        protected async Task<IReadOnlyList<TDto>> LoadManyAsync(Expression<Func<TEntity, bool>>? search = null)
        {
            var entities = search == null 
                ? await _sqliteDatabaseService.AsyncConnection.Table<TEntity>().ToListAsync()
                : await _sqliteDatabaseService.AsyncConnection.Table<TEntity>().Where(search).ToListAsync();

            var dtos = new TDto[entities.Count];
            for (var i = 0; i < dtos.Length; i++)
            {
                dtos[i] = await PopulateDtoAsync(entities[i]);
            }

            return dtos;
        }

        protected async Task<TDto?> LoadOneAsync(TId id)
        {
            var entity = await _sqliteDatabaseService.AsyncConnection.Table<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
            {
                return null;
            }

            var dto = await PopulateDtoAsync(entity);
            return dto;
        }
         
        //todo how do we set SyncState for synced entities?
        protected virtual Task<TEntity> PopulateEntityAsync(TDto dto)
        {
            return Task.FromResult(new TEntity
            {
                Id = dto.Id,
                EntityState = dto.EntityState,
                ModifiedAtTicks = dto.ModifiedAtTicks,
                SerializedDto = JsonConvert.SerializeObject(dto),
            }); 
        }

        protected virtual Task<TDto> PopulateDtoAsync(TEntity entity)
        {
            var dto = JsonConvert.DeserializeObject<TDto>(entity.SerializedDto);
            if (dto == null)
            {
                throw new InvalidOperationException($"Failed to deserialize {typeof(TDto).Name} from {typeof(TEntity).Name}");
            }

            return Task.FromResult(dto);
        }
    }
}