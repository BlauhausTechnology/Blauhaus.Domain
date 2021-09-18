using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncClientRepositoryTests
{
    public class SaveSyncedDtosAsyncTests : BaseSqliteTest<SyncClientRepository<ISqliteTestModel, ISqliteTestDto, SqliteTestSyncCommand, SqliteTestRootEntity>>
    {
        private Guid _rootId1;
        private Guid _rootId2;
        private Guid _rootId3;

        public override void Setup()
        {
            base.Setup();

            _rootId1 = Guid.NewGuid();
            _rootId2 = Guid.NewGuid();
            _rootId3 = Guid.NewGuid();

            var entities = new List<SqliteTestRootEntity>
            {
                new SqliteTestRootEntity{ Id = _rootId1 },
                new SqliteTestRootEntity{ Id = _rootId2 },
                new SqliteTestRootEntity{ Id = _rootId3 }
            };

            var connection = SqliteDatabaseService.AsyncConnection;
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns(new SqliteTestRootEntity(), new List<ISyncClientEntity<Guid>>());
            connection.InsertAllAsync(entities);
        }

        [Test]
        public async Task IF_root_entity_already_exists_SHOULD_update_and_set_sync_state_to_InSync()
        {
            //Arrange
            var bob = new SqliteTestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Bob",
                SyncState = SyncState.InSync
            };
            await Connection.InsertAsync(bob);
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns_root(new SqliteTestRootEntity
            {
                Id = bob.Id,
                RootName = "Fred",
                SyncState = SyncState.OutOfSync
            });

            //Act
            var dto = new MockBuilder<ISqliteTestDto>().With(x => x.Id, bob.Id).Object;
            await Sut.SaveSyncedDtosAsync(new List<ISqliteTestDto>{dto});

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
            var newBob = await Connection.Table<SqliteTestRootEntity>().FirstAsync(x => x.Id == bob.Id);
            Assert.AreEqual("Fred", newBob.RootName);
            Assert.AreEqual(SyncState.InSync, newBob.SyncState);
        } 
        
        [Test]
        public async Task IF_child_entity_already_exists_SHOULD_update_without_setting_sync_state_to_InSync()
        {
            //Arrange
            var bob = new SqliteTestRootEntity
            {
                Id = Guid.NewGuid(),
            };
            var child = new SqliteTestChildEntity
            {
                Id = Guid.NewGuid(),
                RootEntityId = bob.Id,
                ChildName = "Pop"
            };
            await Connection.InsertAsync(bob);
            await Connection.InsertAsync(child);
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns(new SqliteTestRootEntity(), new List<ISyncClientEntity<Guid>>
            {
                new SqliteTestChildEntity()
                {
                    Id = child.Id,
                    SyncState = SyncState.OutOfSync,
                    ChildName = "Hop"
                }
            });

            //Act
            var dto = new MockBuilder<ISqliteTestDto>().Object;
            await Sut.SaveSyncedDtosAsync(new List<ISqliteTestDto>{dto});

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
            var newChild = await Connection.Table<SqliteTestChildEntity>().FirstAsync(x => x.Id == child.Id);
            Assert.AreEqual("Hop", newChild.ChildName);
            Assert.AreEqual(SyncState.OutOfSync, newChild.SyncState);
        } 
        
        [Test]
        public async Task IF_root_entity_does_not_exist_SHOULD_insert_and_set_sync_state_to_InSync()
        {
            //Arrange  
            var bob = new SqliteTestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Fred",
                SyncState = SyncState.OutOfSync
            };
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns_root(bob);

            //Act
            var dto = new MockBuilder<ISqliteTestDto>().Object;
            await Sut.SaveSyncedDtosAsync(new List<ISqliteTestDto>{dto});

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
            var newBob = await Connection.Table<SqliteTestRootEntity>().FirstAsync(x => x.Id == bob.Id);
            Assert.AreEqual("Fred", newBob.RootName);
            Assert.AreEqual(SyncState.InSync, newBob.SyncState);
        } 
        
        [Test]
        public async Task IF_child_entity_does_not_exist_SHOULD_insert()
        {
            //Arrange
            var bob = new SqliteTestRootEntity
            {
                Id = Guid.NewGuid(),
            };
            var child = new SqliteTestChildEntity
            {
                Id = Guid.NewGuid(),
                RootEntityId = bob.Id,
                ChildName = "Pop",
                SyncState = SyncState.OutOfSync
            };
            await Connection.InsertAsync(bob);
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns(new SqliteTestRootEntity(), new List<ISyncClientEntity<Guid>>
            {
                new SqliteTestChildEntity()
                {
                    Id = child.Id,
                    ChildName = "Hop"
                }
            });

            //Act
            var dto = new MockBuilder<ISqliteTestDto>().Object;
            await Sut.SaveSyncedDtosAsync(new List<ISqliteTestDto>{dto});

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
            var newChild = await Connection.Table<SqliteTestChildEntity>().FirstAsync(x => x.Id == child.Id);
            Assert.AreEqual("Hop", newChild.ChildName);
            Assert.AreEqual(SyncState.OutOfSync, newChild.SyncState);
            Assert.AreEqual(1, await Connection.Table<SqliteTestChildEntity>().CountAsync());
        } 
        
        [Test]
        public async Task SHOULD_reload_child_entities_and_return_model_constructed_by_converter()
        {
            //Arrange
            var bob = new SqliteTestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Fred"
            };
            var alreadyExistingChildEntity = new SqliteTestChildEntity{Id = Guid.NewGuid()};
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns_root(bob);
            MockClientEntityConverter.Where_LoadChildEntities_returns(new List<ISyncClientEntity<Guid>>{alreadyExistingChildEntity});
            var model = new MockBuilder<ISqliteTestModel>().Object;
            MockClientEntityConverter.Where_ConstructModel_returns(model);

            //Act
            var dto = new MockBuilder<ISqliteTestDto>().Object;
            var result =  await Sut.SaveSyncedDtosAsync(new List<ISqliteTestDto>{dto});

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ConstructModel(bob, It.Is<List<ISyncClientEntity<Guid>>>(y => 
                y[0] == alreadyExistingChildEntity)));
            Assert.AreEqual(model, result[0]);
        } 
    }
}