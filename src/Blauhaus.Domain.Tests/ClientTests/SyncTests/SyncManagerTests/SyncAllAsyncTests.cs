using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.TestHelpers.Extensions;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync;
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
            //Act
            await Sut.SyncAllAsync(MockKeyValueProvider);

            //Assert
            MockSyncClient1.Verify(x => x.SyncDtoAsync(MockKeyValueProvider));
        }

        [Test]
        public async Task IF_any_client_fails_SHOULD_return_error()
        {
            //Arrange
            MockSyncClient2.Where_SyncDtoAsync_returns(Response.Failure(Errors.Errors.Cancelled));

            //Act
            var result = await Sut.SyncAllAsync(MockKeyValueProvider);

            //Assert
            Assert.That(result.Error, Is.EqualTo(Errors.Errors.Cancelled));
        }

        [Test]
        public async Task SHOULD_publish_updates()
        {
            //Arrange
            var batch1Dto1 = DtoSyncStatus.Create("DtoOne", 10, 12);
            var batch2Dto1 = batch1Dto1.Update(10, 2);
            var batch3Dto1 = batch2Dto1.Update(2, 0);
            var batch1Dto2 = DtoSyncStatus.Create("DtoTwo", 5, 0);
            MockSyncClient1.Where_SubscribeAsync_publishes(batch1Dto1, batch2Dto1, batch3Dto1);
            MockSyncClient2.Where_SubscribeAsync_publishes(batch1Dto2);
            using var statusUpdates = await Sut.SubscribeToUpdatesAsync();
            
            //Act
            await Sut.SyncAllAsync(MockKeyValueProvider);

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