using System;
using AutoFixture;
using Blauhaus.Common.Utils.Extensions;
using Blauhaus.Domain.Client.Sqlite.DtoCaches;
using Blauhaus.Domain.Client.Sqlite.Ioc;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base
{
    public abstract class BaseSyncDtoCacheTest : BaseSqliteTest<SyncDtoCache<MyDto, Guid, MyCachedDtoEntity>>
    {
        protected MyDto DtoOne = null!;
        protected MyDto DtoTwo = null!;
        protected MyDto DtoThree = null!;

        protected MyCachedDtoEntity CachedDtoOne = null!;
        protected MyCachedDtoEntity CachedDtoTwo = null!;
        protected MyCachedDtoEntity CachedDtoThree = null!;

        public override void Setup()
        {
            base.Setup();

            DtoOne = MyFixture.Create<MyDto>().With(x => x.Name, "Bob").With(x => x.ModifiedAtTicks, 1000);
            DtoTwo = MyFixture.Create<MyDto>().With(x => x.Name, "Frank").With(x => x.ModifiedAtTicks, 3000);
            DtoThree = MyFixture.Create<MyDto>().With(x => x.Name, "Bill").With(x => x.ModifiedAtTicks, 2000);

            CachedDtoOne = new MyCachedDtoEntity().FromDto(DtoOne);
            CachedDtoTwo = new MyCachedDtoEntity().FromDto(DtoTwo);
            CachedDtoThree = new MyCachedDtoEntity().FromDto(DtoThree);

            
        }
    }
}