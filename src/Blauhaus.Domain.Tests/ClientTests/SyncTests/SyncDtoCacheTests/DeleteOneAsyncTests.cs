using Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests.Base;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests
{
    public class DeleteOneAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task IF_Dto_exists_SHOULD_delete()
        {
            //Arrange
            await Connection.InsertAsync(SyncedDtoEntityThree);
            
            //Act
            await Sut.DeleteOneAsync(DtoThree.Id);
            
            //Assert
            Assert.That(await Sut.TryGetOneAsync(DtoThree.Id), Is.Null);
        }
    }
}