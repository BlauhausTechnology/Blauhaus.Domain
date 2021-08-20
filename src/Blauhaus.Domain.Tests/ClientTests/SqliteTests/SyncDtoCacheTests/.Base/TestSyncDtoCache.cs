using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Sqlite.DtoCaches;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base
{
    public class TestSyncDtoCache : SyncDtoCache<MyDto, MyCachedDtoEntity, Guid>
    {
        public TestSyncDtoCache(IAnalyticsService analyticsService, ISqliteDatabaseService sqliteDatabaseService) : base(analyticsService, sqliteDatabaseService)
        {
        }

        protected override async Task<MyCachedDtoEntity> PopulateEntityAsync(MyDto dto)
        {
            var entity = await base.PopulateEntityAsync(dto);
            entity.Name = dto.Name;
            return entity;
        }
    }
}