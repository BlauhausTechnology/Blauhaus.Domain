using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.TestHelpers.EFCore.Extensions;
using Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests.Base;
using Blauhaus.Domain.Tests.TestObjects.Server;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests
{
    public class GetAllAsyncTests : BaseMyServerRepositoryDtoTest
    {

        [Test]
        public async Task SHOULD_load_entities()
        { 
            //Arrange
            AdditionalSetup(context => context.Seed(new MyServerEntityBuilder().With(x => x.Name, "Fred").Object));

            //Act 
            var result = await Sut.GetAllAsync();

            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.FirstOrDefault(x => x.Name == "Fred"), Is.Not.Null);

        }   
    }
}