using NUnit.Framework;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using System.Collections.Generic;
using System.Linq;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests
{
    public class HandleAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task SHOULD_add_Dto_to_Cache()
        {
            //Act
            await Sut.HandleAsync(DtoOne);
            
            //Assert
            Assert.That(await Sut.GetOneAsync(DtoOne.Id), Is.Not.Null);
        }

        [Test]
        public async Task SHOULD_not_add_duplicate()
        {
            //Act
            await Sut.HandleAsync(DtoOne);
            await Sut.HandleAsync(DtoOne);
            
            //Assert
            var all = await Sut.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task SHOULD_update_subscribers_to_that_id()
        {
            //Arrange
            var publishedDtos = new List<MyDto>();
            await Sut.SubscribeAsync(d =>
            {
                publishedDtos.Add(d);
                return Task.CompletedTask;
            }, dto => dto.Id == DtoOne.Id);
            
            //Act
            await Sut.HandleAsync(DtoOne);
            
            //Assert
            Assert.That(publishedDtos.Count, Is.EqualTo(1));
            Assert.That(publishedDtos.First(), Is.EqualTo(DtoOne));
        }

        [Test]
        public async Task SHOULD_NOT_update_subscribers_to_different_id()
        {
            //Arrange
            var publishedDtos = new List<MyDto>();
            await Sut.SubscribeAsync(d =>
            {
                publishedDtos.Add(d);
                return Task.CompletedTask;
            }, dto => dto.Id == DtoOne.Id);
            
            //Act
            await Sut.HandleAsync(DtoThree);
            
            //Assert
            Assert.That(publishedDtos.Count, Is.EqualTo(0));
        }
    }
}