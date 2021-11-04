using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Time.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using Blauhaus.Domain.Abstractions.Actors;

namespace Blauhaus.Domain.Server.EFCore.Actors
{
    public abstract class BaseEntityDtoModelActor<TDbContext, TEntity, TModel, TDto> : BaseEntityModelActor<TDbContext, TEntity, TModel>, IDtoModelActor<TModel, TDto, Guid>
        where TDbContext : DbContext
        where TModel : IHasId<Guid> 
        where TDto : IHasId<Guid>
        where TEntity : BaseServerEntity, IDtoOwner<TDto>
    {

        protected readonly IDtoHandler<TDto> DtoHandler;
        public Task<TDto> GetDtoAsync() => LoadedEntity.GetDtoAsync();

        protected BaseEntityDtoModelActor(
            Func<TDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService,
            IDtoHandler<TDto> dtoHandler) 
            : base(dbContextFactory, analyticsService, timeService)
        {
            DtoHandler = dtoHandler;
        }
        
        protected override async Task<TModel> LoadModelAsync()
        {
            var dto = await GetDtoAsync();
            return await PopulateModelAsync(dto);
        }

        protected abstract Task<TModel> PopulateModelAsync(TDto dto);
    }
}