using AutoFixture;
using Blauhaus.Common.Utils.Extensions;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.TestHelpers.Builders.Base;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests.Base
{
    public abstract class BaseSyncDtoCacheTest : BaseSqliteTest<TestSyncDtoCache>
    {
        protected MyDto DtoOne = null!;
        protected MyDto DtoTwo = null!;
        protected MyDto DtoThree = null!;

        protected MySyncedDtoEntity SyncedDtoOne = null!;
        protected MySyncedDtoEntity SyncedDtoTwo = null!;
        protected MySyncedDtoEntity SyncedDtoThree = null!;

        public override void Setup()
        {
            base.Setup();

            DtoOne = MyFixture.Build<MyDto>().With(x => x.Name, "Bob").With(x => x.ModifiedAtTicks, 1000).Create();
            DtoTwo = MyFixture.Build<MyDto>().With(x => x.Name, "Frank").With(x => x.ModifiedAtTicks, 3000).Create();
            DtoThree = MyFixture.Build<MyDto>().With(x => x.Name, "Bill").With(x => x.ModifiedAtTicks, 2000).Create();


            SyncedDtoOne = new MySyncedDtoEntity(DtoOne);
            SyncedDtoTwo = new MySyncedDtoEntity(DtoTwo);
            SyncedDtoThree = new MySyncedDtoEntity(DtoThree);

            
        }
    }
}