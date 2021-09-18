using NUnit.Framework;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests.Base;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests
{
    public class LoadLastModifiedAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task IF_Dtos_exist_SHOULD_return_all()
        {
            //Arrange
            await Connection.InsertAsync(SyncedDtoOne);
            await Connection.InsertAsync(SyncedDtoTwo);
            await Connection.InsertAsync(SyncedDtoThree);
            
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