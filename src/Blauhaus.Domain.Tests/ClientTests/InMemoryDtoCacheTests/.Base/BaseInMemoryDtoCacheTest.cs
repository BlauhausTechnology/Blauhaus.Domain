using AutoFixture;
using Blauhaus.Common.Utils.Extensions;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base
{
    public class BaseInMemoryDtoCacheTest : BaseDomainTest<TestInMemoryDtoCache>
    {
        protected MyDto DtoOne;
        protected MyDto DtoTwo;
        protected MyDto DtoThree;

        public override void Setup()
        {
            base.Setup();

            DtoOne = MyFixture.Create<MyDto>().With(x => x.Name, "Bob");
            DtoTwo = MyFixture.Create<MyDto>().With(x => x.Name, "Frank");
            DtoThree = MyFixture.Create<MyDto>().With(x => x.Name, "Bill");
            
        }

    }
}