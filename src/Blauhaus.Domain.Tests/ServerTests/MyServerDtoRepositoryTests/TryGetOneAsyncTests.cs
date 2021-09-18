using System;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests
{
    public class TryGetOneAsyncTests : BaseMyServerRepositoryDtoTest
    {

        [Test]
        public async Task SHOULD_load_entity()
        { 
            //Act 
            var result = await Sut.TryGetOneAsync(ExistingEntityId);

            //Assert
            Assert.That(result, Is.Not.Null);
            var existingEntity = ExistingEntityBuilder.Object;
            Assert.That(result!.Name, Is.EqualTo(existingEntity.Name));
            Assert.That(result.ModifiedAtTicks, Is.EqualTo(existingEntity.ModifiedAt.Ticks));
            Assert.That(result.EntityState, Is.EqualTo(existingEntity.EntityState));
        } 
        
        [Test]
        public async Task IF_entity_does_not_exist_SHOULD_return_null()
        { 
            //Act 
            var result = await Sut.TryGetOneAsync(Guid.NewGuid());

            //Assert
            Assert.That(result, Is.Null);
        } 
    }
}