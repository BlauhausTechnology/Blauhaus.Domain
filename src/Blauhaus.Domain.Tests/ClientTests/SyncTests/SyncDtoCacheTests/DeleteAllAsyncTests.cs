using Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests.Base;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests
{
    public class DeleteAllAsyncTests : BaseSyncDtoCacheTest
    {
        [Test]
        public async Task SHOULD_delete_all()
        {
            //Arrange
            await Connection.InsertAsync(SyncedDtoEntityOne);
            await Connection.InsertAsync(SyncedDtoEntityTwo);
            await Connection.InsertAsync(SyncedDtoEntityThree);
            
            //Act
            await Sut.DeleteAllAsync();
            
            //Assert
            Assert.That((await Sut.GetAllAsync()).Count, Is.EqualTo(0));
        }
    }
}