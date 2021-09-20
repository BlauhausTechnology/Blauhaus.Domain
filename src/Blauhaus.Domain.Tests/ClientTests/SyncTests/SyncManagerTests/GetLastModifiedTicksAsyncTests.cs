using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncManagerTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncManagerTests
{
    public class GetLastModifiedTicksAsyncTests : BaseSyncManagerTest
    {
        [Test]
        public async Task SHOULD_collate_results_from_DtoSyncClients()
        {
            //Arrange
            MockSyncClient1.Where_GetLastModifiedTicksAsync_returns("Dto1", 2000);
            MockSyncClient2.Where_GetLastModifiedTicksAsync_returns("Dto2", 3000);
            MockSyncClient3.Where_GetLastModifiedTicksAsync_returns("Dto3", 1000);

            //Act
            var result = await Sut.GetLastModifiedTicksAsync(MockKeyValueProvider);

            //Assert
            Assert.That(result["Dto1"], Is.EqualTo(2000));
            Assert.That(result["Dto2"], Is.EqualTo(3000));
            Assert.That(result["Dto3"], Is.EqualTo(1000));

        }
    }
}