using Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests
{
    public class GetAllAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task IF_Dtos_exist_SHOULD_return_all()
        {
            //Arrange
            await Connection.InsertAsync(CachedDtoOne);
            await Connection.InsertAsync(CachedDtoThree);
            
            //Act
            var result = await Sut.GetAllAsync();
            
            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(DtoOne.Id));
            Assert.That(result[1].Id, Is.EqualTo(DtoThree.Id));
        }
        
        [Test]
        public async Task IF_no_Dtos_exist_SHOULD_return_empty_list()
        {
            //Act
            var result = await Sut.GetAllAsync();
            
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}