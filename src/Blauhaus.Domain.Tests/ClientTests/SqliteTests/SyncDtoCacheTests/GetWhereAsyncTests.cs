using Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests
{
    public class GetWhereAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task IF_Dtos_exist_SHOULD_return_matching()
        {
            //Arrange
            await Connection.InsertAsync(CachedDtoOne);
            await Connection.InsertAsync(CachedDtoTwo);
            await Connection.InsertAsync(CachedDtoThree);
            
            //Act
            var result = await Sut.GetWhereAsync(x => x.Name.StartsWith("B"));
            
            //Assert
            Assert.That(result[0].Id, Is.EqualTo(DtoOne.Id));
            Assert.That(result[1].Id, Is.EqualTo(DtoThree.Id));
        }
        

        [Test]
        public async Task IF_Dtos_exist_but_none_match_SHOULD_return_empty_list()
        {
            //Arrange
            await Connection.InsertAsync(CachedDtoOne);
            await Connection.InsertAsync(CachedDtoTwo);
            await Connection.InsertAsync(CachedDtoThree);
            
            //Act
            var result = await Sut.GetWhereAsync(x => x.Name.StartsWith("XXX"));
            
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
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