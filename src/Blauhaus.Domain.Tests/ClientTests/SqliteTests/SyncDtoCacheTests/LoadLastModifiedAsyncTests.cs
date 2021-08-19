using Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests
{
    public class LoadLastModifiedAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task IF_Dtos_exist_SHOULD_return_all()
        {
            //Arrange
            await Connection.InsertAsync(CachedDtoOne);
            await Connection.InsertAsync(CachedDtoTwo);
            await Connection.InsertAsync(CachedDtoThree);
            
            //Act
            var result = await Sut.LoadLastModifiedAsync();
            
            //Assert
            Assert.That(result, Is.EqualTo(3000));
        }
        
        [Test]
        public async Task IF_no_Dtos_exist_SHOULD_return_0()
        {
            //Act
            var result = await Sut.LoadLastModifiedAsync();
            
            //Assert
            Assert.That(result, Is.EqualTo(0));
        }
    }
}