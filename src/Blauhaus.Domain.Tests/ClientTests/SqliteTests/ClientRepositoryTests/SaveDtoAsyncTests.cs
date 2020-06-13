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
    public class SaveDtodAsyncTests : BaseSqliteTest<ClientRepository<ITestModel, ITestDto, TestRootEntity>>
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
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns_root(new TestRootEntity
            {
                Id = bob.Id,
                RootName = "Fred"
            });

            //Act
            var dto = new MockBuilder<ITestDto>().With(x => x.Id, bob.Id).Object;
            await Sut.SaveDtoAsync(dto);

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
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
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns(new TestRootEntity(), new List<ISyncClientEntity>
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
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
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
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns_root(bob);

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            await Sut.SaveDtoAsync(dto);

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
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
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns(new TestRootEntity(), new List<ISyncClientEntity>
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
            MockClientEntityConverter.Mock.Verify(x => x.ExtractEntitiesFromDto(dto));
            var newChild = await Connection.Table<TestChildEntity>().FirstAsync(x => x.Id == child.Id);
            Assert.AreEqual("Hop", newChild.ChildName);
            Assert.AreEqual(1, await Connection.Table<TestChildEntity>().CountAsync());
        } 
        
        [Test]
        public async Task SHOULD_reload_all_child_entities_and_return_model_constructed_by_entity_converter()
        {
            //Arrange
            var bob = new TestRootEntity
            {
                Id = Guid.NewGuid(),
                RootName = "Fred"
            };
            var boblet = new TestChildEntity{Id = Guid.NewGuid()};
            MockClientEntityConverter.Where_ExtractEntitiesFromDto_returns(bob, new List<ISyncClientEntity>());
            MockClientEntityConverter.Where_LoadChildEntities_returns(new List<ISyncClientEntity>{boblet});
            var model = new MockBuilder<ITestModel>().Object;
            MockClientEntityConverter.Where_ConstructModel_returns(model);

            //Act
            var dto = new MockBuilder<ITestDto>().Object;
            var result = await Sut.SaveDtoAsync(dto); 

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ConstructModel(bob, It.Is<List<ISyncClientEntity>>(y=> 
                y[0] == boblet)));
            Assert.AreEqual(model, result);
        } 
    }
}