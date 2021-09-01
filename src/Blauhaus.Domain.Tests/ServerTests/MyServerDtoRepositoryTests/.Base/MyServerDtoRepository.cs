using System;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Server.EFCore.Repositories;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.Domain.Tests.TestObjects.Server;
using Blauhaus.Time.Abstractions;

namespace Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests.Base
{
    public class MyServerDtoRepository : BaseServerDtoRepository<MyDbContext, MyDto, Guid>
    {
        public MyServerDtoRepository(
            Func<MyDbContext> dbContextFactory, 
            IAnalyticsService analyticsService, 
            ITimeService timeService) 
                : base(dbContextFactory, analyticsService, timeService)
        {
        }
    }
}