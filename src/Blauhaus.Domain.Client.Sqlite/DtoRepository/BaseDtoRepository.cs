using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.Sqlite.Repository;

namespace Blauhaus.Domain.Client.Sqlite.DtoRepository
{
    public abstract class BaseDtoRepository<TDto, TRootEntity> : BasePublisher, IDtoRepository<TDto> 
        where TDto : class, IClientEntity
        where TRootEntity : class, ISyncClientEntity
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISqliteDatabaseService _databaseService;
        private readonly IDtoConverter<TDto, TRootEntity> _dtoConverter;

        protected BaseDtoRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IDtoConverter<TDto, TRootEntity> dtoConverter)
        {
            _analyticsService = analyticsService;
            _databaseService = sqliteDatabaseService;
            _dtoConverter = dtoConverter;

        }
        
        public Task<IDisposable> SubscribeAsync(Func<TDto, Task> handler, Func<TDto, bool>? filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<TDto?> LoadByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<TDto> SaveDtoAsync(TDto dto)
        {
            throw new NotImplementedException();
        }

    }
}