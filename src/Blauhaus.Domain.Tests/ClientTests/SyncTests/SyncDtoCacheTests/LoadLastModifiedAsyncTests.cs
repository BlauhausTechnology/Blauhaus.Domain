using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests
{
    public class LoadLastModifiedAsyncTests : BaseSyncDtoCacheTest
    {
        
        [Test]
        public async Task IF_no_Dtos_exist_SHOULD_return_0()
        {
            //Act
            var result = await Sut.LoadLastModifiedTicksAsync();
            
            //Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task IF_Dtos_exist_SHOULD_return_most_recently_modified()
        {
            //Arrange
            await Connection.InsertAsync(SyncedDtoEntityOne);
            await Connection.InsertAsync(SyncedDtoEntityTwo);
            await Connection.InsertAsync(SyncedDtoEntityThree);
            
            //Act
            var result = await Sut.LoadLastModifiedTicksAsync();
            
            //Assert
            Assert.That(result, Is.EqualTo(3000));
        }

        [Test]
        public async Task IF_entity_is_not_Synced_SHOULD_ignore()
        {
            //Arrange
            SyncedDtoEntityTwo.SyncState = SyncState.OutOfSync;
            await Connection.InsertAsync(SyncedDtoEntityOne);
            await Connection.InsertAsync(SyncedDtoEntityTwo);
            await Connection.InsertAsync(SyncedDtoEntityThree);
            
            //Act
            var result = await Sut.LoadLastModifiedTicksAsync();
            
            //Assert
            Assert.That(result, Is.EqualTo(2000));
        }
         
    }
}