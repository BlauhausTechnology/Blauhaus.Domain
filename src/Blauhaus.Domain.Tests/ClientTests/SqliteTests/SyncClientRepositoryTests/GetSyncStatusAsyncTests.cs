using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using NUnit.Framework;
using SqlKata;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncClientRepositoryTests
{
    public class GetSyncStatusAsyncTests :  BaseSqliteTest<SyncClientRepository<ITestModel, ITestDto, TestSyncCommand, TestRootEntity>>
    {
        private readonly Guid _ziggyId = Guid.NewGuid();
        private TestSyncCommand _syncCommand;

        public override void Setup()
        {
            base.Setup();
            _syncCommand = new TestSyncCommand();
            MockSyncQueryGenerator.Where_GenerateQuery_returns(() => new Query(nameof(TestRootEntity)));
        }

        [Test]
        public async Task WHEN_DB_contains_entities_SHOULD_return_correct_data()
        {
            //Arrange
            var entities = new List<TestRootEntity>
            {
                new TestRootEntity{ModifiedAtTicks = 2000, RootName = "log"},
                new TestRootEntity{ModifiedAtTicks = 1000, RootName = "nog"},
                new TestRootEntity{ModifiedAtTicks = 3000, RootName = "Jiggy"},
                new TestRootEntity{ModifiedAtTicks = 5000, RootName = "Ziggy", Id = _ziggyId},
                new TestRootEntity{ModifiedAtTicks = 4000, RootName = ""},
            };
            await Connection.InsertAllAsync(entities); 

            //Act
            var result = await Sut.GetSyncStatusAsync(_syncCommand);

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
            var result = await Sut.GetSyncStatusAsync(_syncCommand);

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
            var result = await Sut.GetSyncStatusAsync(_syncCommand);

            //Arrance
            Assert.AreEqual(0, result.AllLocalEntities);
            Assert.AreEqual(0, result.OldestModifiedAt);
            Assert.AreEqual(null, result.NewestModifiedAt);
        }

        [Test]
        public async Task WHEN_QueryGenerator_modifies_query_SHOULD_apply()
        {
            //Arrange
            var entities = new List<TestRootEntity>
            {
                new TestRootEntity{ModifiedAtTicks = 2000, RootName = "log"},
                new TestRootEntity{ModifiedAtTicks = 1000, RootName = "nog"},
                new TestRootEntity{ModifiedAtTicks = 3000, RootName = "Jiggy"},
                new TestRootEntity{ModifiedAtTicks = 5000, RootName = "Ziggy", Id = _ziggyId},
                new TestRootEntity{ModifiedAtTicks = 4000, RootName = ""},
            };
            await Connection.InsertAllAsync(entities); 
            MockSyncQueryGenerator.Where_GenerateQuery_returns(() => new Query(nameof(TestRootEntity))
                .WhereContains(nameof(TestRootEntity.RootName), "ggy"));

            //Act
            var result = await Sut.GetSyncStatusAsync(_syncCommand);

            //Assert
            Assert.AreEqual(2, result.AllLocalEntities);
            Assert.AreEqual(3000, result.OldestModifiedAt);
            Assert.AreEqual(5000, result.NewestModifiedAt);
        } 

    }
}