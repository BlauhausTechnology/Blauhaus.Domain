using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests._.Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests._.Base;
using Blauhaus.Errors;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncDtoCacheTests
{
    public class GetOneAsyncTests : BaseSyncDtoCacheTest
    {

        [Test]
        public async Task IF_Dto_exists_SHOULD_return()
        {
            //Arrange
            await Connection.InsertAsync(CachedDtoThree);
            
            //Act
            var result = await Sut.GetOneAsync(DtoThree.Id);
            
            //Assert
            Assert.That(JsonConvert.SerializeObject(result), Is.EqualTo(JsonConvert.SerializeObject(DtoThree)));
        }
        
        [Test]
        public void IF_Dto_does_not_existS_SHOULD_throw()
        { 
            //Act
            Assert.ThrowsAsync<ErrorException>(async ()=> await Sut.GetOneAsync(DtoOne.Id));
        }
         
    }
}