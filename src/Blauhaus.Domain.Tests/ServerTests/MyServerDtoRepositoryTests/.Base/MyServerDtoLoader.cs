using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Server.EFCore.Repositories;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.Domain.Tests.TestObjects.Server;
using Blauhaus.Time.Abstractions;

namespace Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests.Base
{
    public class MyServerDtoLoader : BaseServerDtoLoader<MyDbContext, MyDto, MyServerEntity, Guid>
    {
        public MyServerDtoLoader(
            Func<MyDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
                : base(dbContextFactory, analyticsService, timeService)
        {
        }
         
        protected override Task<MyDto> PopulateDtoAsync(MyServerEntity entity)
        {
            return Task.FromResult(new MyDto
            {
                EntityState = entity.EntityState,
                Id = entity.Id,
                ModifiedAtTicks = entity.ModifiedAt.Ticks,
                Name = entity.Name
            });
        }
    }
}