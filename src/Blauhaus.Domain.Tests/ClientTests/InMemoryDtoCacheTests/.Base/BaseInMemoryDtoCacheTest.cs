using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.TestHelpers.Builders.Base;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base
{
    public class BaseInMemoryDtoCacheTest : BaseDomainTest<TestInMemoryDtoCache>
    {
        protected MyDto DtoOne = null!;
        protected MyDto DtoTwo = null!;
        protected MyDto DtoThree = null!;

        public override void Setup()
        {
            base.Setup();

            DtoOne = new FixtureBuilder<MyDto>().With(x => x.Name, "Bob").Object;
            DtoTwo = new FixtureBuilder<MyDto>().With(x => x.Name, "Frank").Object;
            DtoThree = new FixtureBuilder<MyDto>().With(x => x.Name, "Bill").Object; 
            
        }

    }
}