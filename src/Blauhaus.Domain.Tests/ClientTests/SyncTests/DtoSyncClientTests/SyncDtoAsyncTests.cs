using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.TestHelpers.Extensions;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Tests.ClientTests.SyncTests.DtoSyncClientTests.Base;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.DtoSyncClientTests
{
    public class SyncDtoAsyncTests : BaseDtoSyncClientTest
    {
        private const long Num = 10000000;

        public override void Setup()
        {
            base.Setup();

            MockSyncCommandHandler.Where_HandleAsync_returns(new DtoBatch<MyDto, Guid>(Array.Empty<MyDto>(), 0));
        }

        [Test]
        public async Task SHOULD_load_from_cache()
        {
            //Arrange
            var cacheLastModified = DateTime.UtcNow.AddSeconds(-1).Ticks;
            MockSyncDtoCache.Where_LoadLastModifiedTicksAsync_returns(cacheLastModified);

            //Act
            await Sut.SyncDtoAsync(MockKeyValueProvider);

            //Assert
            MockSyncCommandHandler.Verify_HandleAsync_called_With(x => x.ModifiedAfterTicks == cacheLastModified);
        }
         
        [Test]
        public async Task SHOULD_keep_downloading_and_notifying_until_all_completed()
        {
            //Arrange
            var cacheLastModified = DateTime.UtcNow.AddSeconds(-1).Ticks;
            MockSyncDtoCache.Where_LoadLastModifiedTicksAsync_returns(cacheLastModified);
            MockSyncCommandHandler.Where_HandleAsync_returns_sequence(new List<IDtoBatch<MyDto>>
            {
                new DtoBatch<MyDto, Guid>(new []
                {
                    new MyDto{ ModifiedAtTicks = 1 * Num },
                    new MyDto{ ModifiedAtTicks = 2 * Num },
                }, 5),
                new DtoBatch<MyDto, Guid>(new []
                {
                    new MyDto{ ModifiedAtTicks = 3 * Num },
                    new MyDto{ ModifiedAtTicks = 4 * Num },
                }, 3),
                new DtoBatch<MyDto, Guid>(new []
                {
                    new MyDto{ ModifiedAtTicks = 5 * Num },
                    new MyDto{ ModifiedAtTicks = 6 * Num },
                }, 1),
                new DtoBatch<MyDto, Guid>(new []
                {
                    new MyDto{ ModifiedAtTicks = 7 * Num },
                }, 0)
            });
            using var publishedStatuses = await Sut.SubscribeToUpdatesAsync();
            
            //Act
            await Sut.SyncDtoAsync(MockKeyValueProvider);

            //Assert
            Assert.That(publishedStatuses.Count, Is.EqualTo(4));
                
            Assert.That(publishedStatuses[0].TotalDtoCount, Is.EqualTo(7));
            Assert.That(publishedStatuses[0].CurrentDtoCount, Is.EqualTo(2));
            Assert.That(publishedStatuses[0].DownloadedDtoCount, Is.EqualTo(2));
            Assert.That(publishedStatuses[0].RemainingDtoCount, Is.EqualTo(5));
                
            Assert.That(publishedStatuses[1].TotalDtoCount, Is.EqualTo(7));
            Assert.That(publishedStatuses[1].CurrentDtoCount, Is.EqualTo(2));
            Assert.That(publishedStatuses[1].DownloadedDtoCount, Is.EqualTo(4));
            Assert.That(publishedStatuses[1].RemainingDtoCount, Is.EqualTo(3));
                
            Assert.That(publishedStatuses[2].TotalDtoCount, Is.EqualTo(7));
            Assert.That(publishedStatuses[2].CurrentDtoCount, Is.EqualTo(2));
            Assert.That(publishedStatuses[2].DownloadedDtoCount, Is.EqualTo(6));
            Assert.That(publishedStatuses[2].RemainingDtoCount, Is.EqualTo(1));
                
            Assert.That(publishedStatuses[3].TotalDtoCount, Is.EqualTo(7));
            Assert.That(publishedStatuses[3].CurrentDtoCount, Is.EqualTo(1));
            Assert.That(publishedStatuses[3].DownloadedDtoCount, Is.EqualTo(7));
            Assert.That(publishedStatuses[3].RemainingDtoCount, Is.EqualTo(0));

            MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(0, x => x.ModifiedAfterTicks == cacheLastModified);
            MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(1, x => x.ModifiedAfterTicks == 2 * Num);
            MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(2, x => x.ModifiedAfterTicks == 4 * Num);
            MockSyncCommandHandler.Verify_HandleAsync_called_in_sequence(3, x => x.ModifiedAfterTicks == 6 * Num);

            MockSyncCommandHandler.Verify_HandleAsync_called_With(x => x.DtoName == "MyDto");
        }

        
        [Test]
        public async Task IF_sync_command_handler_fails_on_initial_sync_SHOULD_return_error()
        {
            //Arrange
            MockSyncCommandHandler.Where_HandleAsync_returns_sequence(new List<Response<IDtoBatch<MyDto>>>
            { 
                Response.Failure<IDtoBatch<MyDto>>(Errors.Errors.InvalidValue("Bob"))
            });
            
            //Act
            var result = await Sut.SyncDtoAsync(MockKeyValueProvider);

            //Assert
            Assert.That(result.Error, Is.EqualTo(Errors.Errors.InvalidValue("Bob")));
        }

        [Test]
        public async Task IF_sync_command_handler_fails_on_later_sync_SHOULD_return_error()
        {
            //Arrange
            MockSyncCommandHandler.Where_HandleAsync_returns_sequence(new List<Response<IDtoBatch<MyDto>>>
            {
                Response.Success<IDtoBatch<MyDto>>(new DtoBatch<MyDto, Guid>(new []
                {
                    new MyDto{ ModifiedAtTicks = 1 * Num },
                    new MyDto{ ModifiedAtTicks = 2 * Num },
                }, 5)), 
                Response.Failure<IDtoBatch<MyDto>>(Errors.Errors.InvalidValue("Fred"))
            });
            
            //Act
            var result = await Sut.SyncDtoAsync(MockKeyValueProvider);

            //Assert
            Assert.That(result.Error, Is.EqualTo(Errors.Errors.InvalidValue("Fred")));
        }
    }
}