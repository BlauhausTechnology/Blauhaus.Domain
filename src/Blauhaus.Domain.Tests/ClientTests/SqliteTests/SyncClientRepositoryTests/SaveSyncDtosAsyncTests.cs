using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.SyncClientRepositoryTests
{
    public class SaveSyncDtosAsyncTests : BaseSqliteTest<SyncClientRepository<ITestModel, ITestDto, TestSyncCommand, TestRootEntity>>
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

            var entities = new List<TestRootEntity>
            {
                new TestRootEntity{ Id = _rootId1 },
                new TestRootEntity{ Id = _rootId2 },
                new TestRootEntity{ Id = _rootId3 }
            };

            var connection = SqliteDatabaseService.GetDatabaseConnectionAsync().Result;
            connection.InsertAllAsync(entities);
        }

        [Test]
        public async Task IF_root_entity_already_exists_SHOULD_update()
        {
            //Arrange
            var bob = new TestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Bob"
            };
            await Connection.InsertAsync(bob);
            MockClientEntityManager.Where_ExtractRootEntityFromDto_returns(new TestRootEntity
            {
                Id = bob.Id,
                RootName = "Fred"
            });

            //Act
            var dto = new MockBuilder<ITestDto>().With(x => x.Id, bob.Id).Object;
            await Sut.SaveSyncedDtosAsync(new List<ITestDto>{dto});

            //Assert
            MockClientEntityManager.Mock.Verify(x => x.ExtractRootEntityFromDto(dto));
            var newBob = await Connection.Table<TestRootEntity>().FirstAsync(x => x.Id == bob.Id);
            Assert.AreEqual("Fred", newBob.RootName);
        } 

        
        [Test]
        public async Task IF_child_entity_already_exists_SHOULD_update()
        {
            //Arrange
            var bob = new TestRootEntity
            {
                Id = Guid.NewGuid(),
            };
            var child = new TestChildEntity
            {
                Id = Guid.NewGuid(),
                RootEntityId = bob.Id,
                ChildName = "Pop"
            };
            await Connection.InsertAsync(bob);
            await Connection.InsertAsync(child);
            MockClientEntityManager.Where_ExtractChildEntitiesFromDto_returns(new List<IClientEntity>
            {
                new TestChildEntity()
                {
                    Id = child.Id,
                    ChildName = "Hop"
                }
            });

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveSyncedDtosAsync(new List<ITestDto>{dto});

            //Assert
            MockClientEntityManager.Mock.Verify(x => x.ExtractChildEntitiesFromDto(dto));
            var newChild = await Connection.Table<TestChildEntity>().FirstAsync(x => x.Id == child.Id);
            Assert.AreEqual("Hop", newChild.ChildName);
        } 

        
        [Test]
        public async Task IF_root_entity_does_not_exist_SHOULD_insert()
        {
            //Arrange  
            var bob = new TestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Fred"
            };
            MockClientEntityManager.Where_ExtractRootEntityFromDto_returns(bob);

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveSyncedDtosAsync(new List<ITestDto>{dto});

            //Assert
            MockClientEntityManager.Mock.Verify(x => x.ExtractRootEntityFromDto(dto));
            var newBob = await Connection.Table<TestRootEntity>().FirstAsync(x => x.Id == bob.Id);
            Assert.AreEqual("Fred", newBob.RootName);
        } 
        
        [Test]
        public async Task IF_child_entity_does_not_exist_SHOULD_insert()
        {
            //Arrange
            var bob = new TestRootEntity
            {
                Id = Guid.NewGuid(),
            };
            var child = new TestChildEntity
            {
                Id = Guid.NewGuid(),
                RootEntityId = bob.Id,
                ChildName = "Pop"
            };
            await Connection.InsertAsync(bob);
            MockClientEntityManager.Where_ExtractChildEntitiesFromDto_returns(new List<IClientEntity>
            {
                new TestChildEntity()
                {
                    Id = child.Id,
                    ChildName = "Hop"
                }
            });

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveSyncedDtosAsync(new List<ITestDto>{dto});

            //Assert
            MockClientEntityManager.Mock.Verify(x => x.ExtractChildEntitiesFromDto(dto));
            var newChild = await Connection.Table<TestChildEntity>().FirstAsync(x => x.Id == child.Id);
            Assert.AreEqual("Hop", newChild.ChildName);
            Assert.AreEqual(1, await Connection.Table<TestChildEntity>().CountAsync());
        } 

        
        [Test]
        public async Task SHOULD_return_model_constructed_by_helper()
        {
            //Arrange
            var bob = new TestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Fred"
            };
            MockClientEntityManager.Where_ExtractRootEntityFromDto_returns(bob);
            var model = new MockBuilder<ITestModel>().Object;
            MockClientEntityManager.Where_ConstructModelFromRootEntity_returns(model);

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            var result =  await Sut.SaveSyncedDtosAsync(new List<ITestDto>{dto});

            //Assert
            MockClientEntityManager.Mock.Verify(x => x.ConstructModelFromRootEntity(bob, It.IsAny<SQLiteConnection>()));
            Assert.AreEqual(model, result[0]);
        } 
    }
}