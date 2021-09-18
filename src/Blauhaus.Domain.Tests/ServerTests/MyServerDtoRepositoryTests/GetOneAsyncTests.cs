using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests
{
    public class GetOneAsyncTests : BaseMyServerRepositoryDtoTest
    {

        [Test]
        public async Task SHOULD_load_entity()
        { 
            //Act 
            var result = await Sut.GetOneAsync(ExistingEntityId);

            //Assert
            var existingEntity = ExistingEntityBuilder.Object;
            Assert.That(result.Name, Is.EqualTo(existingEntity.Name));
            Assert.That(result.ModifiedAtTicks, Is.EqualTo(existingEntity.ModifiedAt.Ticks));
            Assert.That(result.EntityState, Is.EqualTo(existingEntity.EntityState));

        }   
    }
}