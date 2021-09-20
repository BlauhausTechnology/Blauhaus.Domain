using AutoFixture;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests.Base
{
    public abstract class BaseSyncDtoCacheTest : BaseSqliteTest<TestSyncDtoCache>
    {
        protected MyDto DtoOne = null!;
        protected MyDto DtoTwo = null!;
        protected MyDto DtoThree = null!;

        protected MySyncedDtoEntity SyncedDtoEntityOne = null!;
        protected MySyncedDtoEntity SyncedDtoEntityTwo = null!;
        protected MySyncedDtoEntity SyncedDtoEntityThree = null!;
        
        protected IKeyValueProvider MockKeyValueProvider = new MockBuilder<IKeyValueProvider>().Object;
        public override void Setup()
        {
            base.Setup();

            DtoOne = MyFixture.Build<MyDto>().With(x => x.Name, "Bob").With(x => x.ModifiedAtTicks, 1000).Create();
            DtoTwo = MyFixture.Build<MyDto>().With(x => x.Name, "Frank").With(x => x.ModifiedAtTicks, 3000).Create();
            DtoThree = MyFixture.Build<MyDto>().With(x => x.Name, "Bill").With(x => x.ModifiedAtTicks, 2000).Create();


            SyncedDtoEntityOne = new MySyncedDtoEntity(DtoOne);
            SyncedDtoEntityTwo = new MySyncedDtoEntity(DtoTwo);
            SyncedDtoEntityThree = new MySyncedDtoEntity(DtoThree);

            
        }
    }
}