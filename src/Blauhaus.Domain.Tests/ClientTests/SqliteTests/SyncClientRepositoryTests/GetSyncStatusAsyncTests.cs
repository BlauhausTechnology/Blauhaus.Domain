using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncClientRepositoryTests
{
    public class GetSyncStatusAsyncTests :  BaseSqliteTest<SyncClientRepository<ITestModel, ITestDto, TestSyncCommand, TestRootEntity>>
    {
        [Test]
        public async Task WHEN_DB_contains_entities_SHOULD_return_correct_data()
        {
            //Arrange
            var entities = new List<TestRootEntity>
            {
                new TestRootEntity{ModifiedAtTicks = 2000},
                new TestRootEntity{ModifiedAtTicks = 1000},
                new TestRootEntity{ModifiedAtTicks = 3000},
                new TestRootEntity{ModifiedAtTicks = 5000},
                new TestRootEntity{ModifiedAtTicks = 4000},
            };
            await Connection.InsertAllAsync(entities); 

            //Act
            var result = await Sut.GetSyncStatusAsync();

            //Arrance
            Assert.AreEqual(5, result.TotalCount);
            Assert.AreEqual(1000, result.FirstModifiedAt);
            Assert.AreEqual(5000, result.LastModifiedAt);
        }

        [Test]
        public async Task SHOULD_ignore_non_synced_entities()
        {
            //Arrange
            var entities = new List<TestRootEntity>
            {
                new TestRootEntity{ModifiedAtTicks = 2000},
                new TestRootEntity{ModifiedAtTicks = 1000, SyncState = SyncState.OutOfSync},
                new TestRootEntity{ModifiedAtTicks = 3000},
                new TestRootEntity{ModifiedAtTicks = 5000, SyncState = SyncState.OutOfSync},
                new TestRootEntity{ModifiedAtTicks = 4000},
            };
            await Connection.InsertAllAsync(entities); 

            //Act
            var result = await Sut.GetSyncStatusAsync();

            //Arrance
            Assert.AreEqual(3, result.TotalCount);
            Assert.AreEqual(2000, result.FirstModifiedAt);
            Assert.AreEqual(4000, result.LastModifiedAt);
        }

        [Test]
        public async Task WHEN_DB_contains_nothing_SHOULD_return_defaults()
        {
            //Act
            var result = await Sut.GetSyncStatusAsync();

            //Arrance
            Assert.AreEqual(0, result.TotalCount);
            Assert.AreEqual(0, result.FirstModifiedAt);
            Assert.AreEqual(null, result.LastModifiedAt);
        }

    }
}