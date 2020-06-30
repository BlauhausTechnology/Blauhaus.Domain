using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using SQLite;
using SqlKata;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncClientRepositoryTests
{
    public class LoadModelsAsyncTests : BaseSqliteTest<SyncClientRepository<ISqliteTestModel, ISqliteTestDto, SqliteTestSyncCommand, SqliteTestRootEntity>>
    {
        private List<SqliteTestRootEntity> _entitiesConstructed;
        private Guid _ziggyId;
        private Guid _twiggyId;

        public override void Setup()
        {
            base.Setup();

            _ziggyId = Guid.NewGuid();
            _twiggyId = Guid.NewGuid();
             
            var entities = new List<SqliteTestRootEntity>
            {
                new SqliteTestRootEntity{ ModifiedAtTicks = 2000, RootName = "Bob" },
                new SqliteTestRootEntity{ ModifiedAtTicks = 1000, RootName = "Fred" },
                new SqliteTestRootEntity{ ModifiedAtTicks = 3000, RootName = "John" },
                new SqliteTestRootEntity{ ModifiedAtTicks = 6000, RootName = "Cedric" },
                new SqliteTestRootEntity{ ModifiedAtTicks = 8000, RootName = "Ramona" },
                new SqliteTestRootEntity{ ModifiedAtTicks = 9000, RootName = "Ziggy", Id = _ziggyId },
                new SqliteTestRootEntity{ ModifiedAtTicks = 5000, RootName = "Twiggy", Id = _twiggyId },
                new SqliteTestRootEntity{ ModifiedAtTicks = 4000, RootName = "Morris" },
            };

            Connection.InsertAllAsync(entities);

            _entitiesConstructed = new List<SqliteTestRootEntity>();
            MockSyncClientSqlQueryGenerator.Where_GenerateQuery_returns(() => new Query(nameof(SqliteTestRootEntity)));
            MockClientEntityConverter.Mock.Setup(x => x.ConstructModel(Capture.In(_entitiesConstructed), It.IsAny<List<ISyncClientEntity>>()))
                .Returns((SqliteTestRootEntity root, IEnumerable<ISyncClientEntity> childEntities)=> new MockBuilder<ISqliteTestModel>()
                    .With(x => x.Id, root.Id)
                    .With(x => x.ModifiedAtTicks, root.ModifiedAtTicks).Object);
        }

        [Test]
        public async Task WHEN_OlderThan_and_batch_size_are_given_SHOULD_return_correct_quantity_in_correct_order()
        {
            //Act
            var result = await Sut.LoadModelsAsync(new SqliteTestSyncCommand
            {
                OlderThan = 6000,
                BatchSize = 3
            });

            //Arrance
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(5000, result[0].ModifiedAtTicks);
            Assert.AreEqual(4000, result [1].ModifiedAtTicks);
            Assert.AreEqual(3000, result[2].ModifiedAtTicks);
            Assert.AreEqual(3, _entitiesConstructed.Count);
            Assert.AreEqual(5000, _entitiesConstructed[0].ModifiedAtTicks);
            Assert.AreEqual(4000, _entitiesConstructed [1].ModifiedAtTicks);
            Assert.AreEqual(3000, _entitiesConstructed[2].ModifiedAtTicks);
            MockAnalyticsService.VerifyTrace("Models loaded");
            MockAnalyticsService.VerifyTraceProperty("Count", 3);
            MockAnalyticsService.VerifyTraceProperty("SQL query", "SELECT * FROM \"SqliteTestRootEntity\" WHERE \"ModifiedAtTicks\" < 6000 ORDER BY \"ModifiedAtTicks\" DESC LIMIT 3");
        } 

        
        [Test]
        public async Task WHEN_NewerThan_and_batch_size_are_given_SHOULD_return_correct_quantity_with_oldest_first()
        {
            //Act
            var result = await Sut.LoadModelsAsync(new SqliteTestSyncCommand
            {
                OlderThan = null,
                NewerThan = 6000,
                BatchSize = 3
            });

            //Arrance
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(9000, result[0].ModifiedAtTicks);
            Assert.AreEqual(8000, result [1].ModifiedAtTicks);
            Assert.AreEqual(2, _entitiesConstructed.Count);
            Assert.AreEqual(9000, _entitiesConstructed[0].ModifiedAtTicks);
            Assert.AreEqual(8000, _entitiesConstructed [1].ModifiedAtTicks);
            MockAnalyticsService.VerifyTrace("Models loaded");
            MockAnalyticsService.VerifyTraceProperty("Count", 2);
            MockAnalyticsService.VerifyTraceProperty("SQL query", "SELECT * FROM \"SqliteTestRootEntity\" WHERE \"ModifiedAtTicks\" > 6000 ORDER BY \"ModifiedAtTicks\" DESC LIMIT 3");

        } 

        [Test]
        public async Task WHEN_OlderThan_and_NewerThan_are_not_given_SHOULD_return_correct_quantity_in_correct_order()
        {
            //Act
            var result = await Sut.LoadModelsAsync(new SqliteTestSyncCommand
            {
                BatchSize = 3
            });

            //Arrance
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(9000, result[0].ModifiedAtTicks);
            Assert.AreEqual(8000, result[1].ModifiedAtTicks);
            Assert.AreEqual(6000, result[2].ModifiedAtTicks);
            Assert.AreEqual(3, _entitiesConstructed.Count);
            Assert.AreEqual(9000, _entitiesConstructed[0].ModifiedAtTicks);
            Assert.AreEqual(8000, _entitiesConstructed [1].ModifiedAtTicks);
            Assert.AreEqual(6000, _entitiesConstructed[2].ModifiedAtTicks);
        } 

        [Test]
        public async Task WHEN_QueryGenerator_modifies_query_SHOULD_apply()
        {
            //Arrange
            MockSyncClientSqlQueryGenerator.Where_GenerateQuery_returns(() => new Query(nameof(SqliteTestRootEntity))
                .WhereContains(nameof(SqliteTestRootEntity.RootName), "ggy"));
            
            //Act
            var result = await Sut.LoadModelsAsync(new SqliteTestSyncCommand { BatchSize = 3 });

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(result.FirstOrDefault(x => x.Id == _twiggyId));
            Assert.IsNotNull(result.FirstOrDefault(x => x.Id == _ziggyId)); 
            Assert.AreEqual(2, _entitiesConstructed.Count);
            Assert.IsNotNull(_entitiesConstructed.FirstOrDefault(x => x.Id == _twiggyId));
            Assert.IsNotNull(_entitiesConstructed.FirstOrDefault(x => x.Id == _ziggyId)); 
        } 
         
    }
}