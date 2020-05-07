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
            Assert.AreEqual(5, result.AllLocalEntities);
            Assert.AreEqual(1000, result.OldestModifiedAt);
            Assert.AreEqual(5000, result.NewestModifiedAt);
        }

        [Test]
        public async Task SHOULD_ignore_ModifiedAts_for_non_synced_entities()
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
            Assert.AreEqual(5, result.AllLocalEntities);
            Assert.AreEqual(3, result.SyncedLocalEntities);
            Assert.AreEqual(2000, result.OldestModifiedAt);
            Assert.AreEqual(4000, result.NewestModifiedAt);
        }

        [Test]
        public async Task WHEN_DB_contains_nothing_SHOULD_return_defaults()
        {
            //Act
            var result = await Sut.GetSyncStatusAsync();

            //Arrance
            Assert.AreEqual(0, result.AllLocalEntities);
            Assert.AreEqual(0, result.OldestModifiedAt);
            Assert.AreEqual(null, result.NewestModifiedAt);
        }

    }
}