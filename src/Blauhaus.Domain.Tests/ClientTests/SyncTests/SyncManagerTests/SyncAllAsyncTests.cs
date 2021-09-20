using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.Extensions;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.Manager;
using Blauhaus.Domain.Client.Sync.SyncClient;
using Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncManagerTests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.Extensions;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncManagerTests
{
    public class SyncAllAsyncTests : BaseSyncManagerTest
    {
        [Test]
        public async Task SHOULD_sync_all_DtoSyncClients()
        {
            //Arrange
            var lastModifieds = new Dictionary<string, long>();

            //Act
            await Sut.SyncAllAsync(lastModifieds, MockKeyValueProvider);

            //Assert
            MockSyncClient1.Verify(x => x.SyncDtoAsync(lastModifieds, MockKeyValueProvider));
        }

        [Test]
        public async Task IF_any_client_fails_SHOULD_return_error()
        {
            //Arrange
            MockSyncClient2.Mock.Setup(x => x.SyncDtoAsync(It.IsAny<Dictionary<string, long>>(), It.IsAny<IKeyValueProvider>()))
                .ReturnsAsync(Response.Failure(Errors.Errors.Cancelled));

            //Act
            var result = await Sut.SyncAllAsync(null, MockKeyValueProvider);

            //Assert
            Assert.That(result.Error, Is.EqualTo(Errors.Errors.Cancelled));
        }

        [Test]
        public async Task SHOULD_publish_updates()
        {
            //Arrange
            var batch1Dto1 = DtoSyncStatus.Create("DtoOne", new MockBuilder<IDtoBatch>()
                .With(x => x.RemainingDtoCount, 12)
                .With(x => x.CurrentDtoCount, 10).Object);
            var batch2Dto1 = batch1Dto1.Update(new MockBuilder<IDtoBatch>()
                .With(x => x.RemainingDtoCount, 2)
                .With(x => x.CurrentDtoCount, 10)
                .Object);
            var batch3Dto1 = batch2Dto1.Update(new MockBuilder<IDtoBatch>()
                .With(x => x.RemainingDtoCount, 0)
                .With(x => x.CurrentDtoCount, 2)
                .Object);
            var batch1Dto2 = DtoSyncStatus.Create("DtoTwo", new MockBuilder<IDtoBatch>()
                .With(x => x.RemainingDtoCount, 0)
                .With(x => x.CurrentDtoCount, 5).Object);
            MockSyncClient1.Where_SubscribeAsync_publishes(batch1Dto1, batch2Dto1, batch3Dto1);
            MockSyncClient2.Where_SubscribeAsync_publishes(batch1Dto2);
            using var statusUpdates = await Sut.SubscribeToUpdatesAsync();
            
            //Act
            await Sut.SyncAllAsync(null, MockKeyValueProvider);

            //Assert
            Assert.That(statusUpdates.Count, Is.EqualTo(4));
            List<OverallSyncStatus> updates = statusUpdates.SerializedUpdates.Select(JsonConvert.DeserializeObject<OverallSyncStatus>).ToList()!;
            
            Assert.That(updates[0].DownloadedDtoCount, Is.EqualTo(10));
            Assert.That(updates[0].TotalDtoCount, Is.EqualTo(22));
            
            Assert.That(updates[1].DownloadedDtoCount, Is.EqualTo(20));
            Assert.That(updates[1].TotalDtoCount, Is.EqualTo(22));
            
            Assert.That(updates[2].DownloadedDtoCount, Is.EqualTo(22));
            Assert.That(updates[2].TotalDtoCount, Is.EqualTo(22));
            
            Assert.That(updates[3].DownloadedDtoCount, Is.EqualTo(27));
            Assert.That(updates[3].TotalDtoCount, Is.EqualTo(27));
        }
    }
}