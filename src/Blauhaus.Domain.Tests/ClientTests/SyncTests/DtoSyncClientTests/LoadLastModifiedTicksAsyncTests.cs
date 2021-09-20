using System;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.SyncTests.DtoSyncClientTests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.DtoSyncClientTests
{
    public class LoadLastModifiedTicksAsyncTests : BaseDtoSyncClientTest
    {
        [Test]
        public async Task SHOULD_populate_from_cache()
        {
            //Arrange
            var lastModified = DateTime.Now.Ticks;
            MockSyncDtoCache.Where_LoadLastModifiedTicksAsync_returns(lastModified);

            //Act
            var result = await Sut.LoadLastModifiedTicksAsync(MockKeyValueProvider);

            //Assert
            Assert.That(result.Key, Is.EqualTo(nameof(MyDto)));
            Assert.That(result.Value, Is.EqualTo(lastModified));

        }
    }
}