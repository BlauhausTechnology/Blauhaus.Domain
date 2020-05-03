using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.ClientRepositoryTests
{
    public class SaveDtodAsyncTests : BaseSqliteTest<ClientRepository<ITestModel, TestRootEntity, ITestDto>>
    {

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
            MockClientRepositoryHelper.Where_ExtractRootEntityFromDto_returns(new TestRootEntity
            {
                Id = bob.Id,
                RootName = "Fred"
            });

            //Act
            var dto = new MockBuilder<ITestDto>().With(x => x.Id, bob.Id).Object;
            await Sut.SaveDtoAsync(dto);

            //Assert
            MockClientRepositoryHelper.Mock.Verify(x => x.ExtractRootEntityFromDto(dto));
            var newBob = await Connection.Table<TestRootEntity>().FirstAsync(x => x.Id == bob.Id);
            Assert.AreEqual("Fred", newBob.RootName);
            Assert.AreEqual(1, await Connection.Table<TestRootEntity>().CountAsync());
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
            MockClientRepositoryHelper.Where_ExtractChildEntitiesFromDto_returns(new List<IClientEntity>
            {
                new TestChildEntity()
                {
                    Id = child.Id,
                    ChildName = "Hop"
                }
            });

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveDtoAsync(dto);

            //Assert
            MockClientRepositoryHelper.Mock.Verify(x => x.ExtractChildEntitiesFromDto(dto));
            var newChild = await Connection.Table<TestChildEntity>().FirstAsync(x => x.Id == child.Id);
            Assert.AreEqual("Hop", newChild.ChildName);
            Assert.AreEqual(1, await Connection.Table<TestChildEntity>().CountAsync());
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
            MockClientRepositoryHelper.Where_ExtractRootEntityFromDto_returns(bob);

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveDtoAsync(dto);

            //Assert
            MockClientRepositoryHelper.Mock.Verify(x => x.ExtractRootEntityFromDto(dto));
            var newBob = await Connection.Table<TestRootEntity>().FirstAsync(x => x.Id == bob.Id);
            Assert.AreEqual("Fred", newBob.RootName);
            Assert.AreEqual(1, await Connection.Table<TestRootEntity>().CountAsync());
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
            MockClientRepositoryHelper.Where_ExtractChildEntitiesFromDto_returns(new List<IClientEntity>
            {
                new TestChildEntity()
                {
                    Id = child.Id,
                    ChildName = "Hop"
                }
            });

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveDtoAsync(dto);

            //Assert
            MockClientRepositoryHelper.Mock.Verify(x => x.ExtractChildEntitiesFromDto(dto));
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
            MockClientRepositoryHelper.Where_ExtractRootEntityFromDto_returns(bob);
            var model = new MockBuilder<ITestModel>().Object;
            MockClientRepositoryHelper.Where_ConstructModelFromRootEntity_returns(model);

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            var result = await Sut.SaveDtoAsync(dto); 

            //Assert
            MockClientRepositoryHelper.Mock.Verify(x => x.ConstructModelFromRootEntity(bob, It.IsAny<SQLiteConnection>()));
            Assert.AreEqual(model, result);
        } 
    }
}