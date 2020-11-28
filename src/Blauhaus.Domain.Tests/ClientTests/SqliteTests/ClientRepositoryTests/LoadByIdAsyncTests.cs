using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.ClientRepositoryTests
{
    public class LoadByIdAsyncTests : BaseSqliteTest<ClientRepository<ISqliteTestModel, ISqliteTestDto, SqliteTestRootEntity>>
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
            connection.InsertAllAsync(entities).Wait();
        }


        [Test]
        public async Task SHOULD_construct_model_using_RootEntity_and_child_entities()
        {
            //Arrange
            MockClientEntityConverter.Where_LoadChildEntities_returns(new List<ISyncClientEntity>
            {
                new SqliteTestChildEntity {ChildName = "Bobby", ModifiedAtTicks = 13}
            });

            //Act
            await Sut.LoadByIdAsync(_rootId2);

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ConstructModel(
                It.Is<SqliteTestRootEntity>(y => y.Id == _rootId2), It.IsAny<List<ISyncClientEntity>>()));
            MockClientEntityConverter.Mock.Verify(x => x.ConstructModel(
                It.IsAny<SqliteTestRootEntity>(), It.Is<List<ISyncClientEntity>>(y => y.First().ModifiedAtTicks == 13)));
        }

        [Test]
        public async Task IF_model_does_not_exist_SHOULD_return_null_without_loading_children()
        { 
            //Act
            var result = await Sut.LoadByIdAsync(Guid.NewGuid());

            //Assert
            Assert.IsNull(result);
            MockClientEntityConverter.Mock.Verify(x => x.LoadChildEntities(It.IsAny<SqliteTestRootEntity>(), It.IsAny<SQLiteConnection>()), Times.Never);
        }

        [Test]
        public async Task SHOULD_return_constructed_model()
        {
            //Arrange
            MockClientEntityConverter.Where_ConstructModel_returns(new MockBuilder<ISqliteTestModel>()
                .With(x => x.RootEntityName, "Bob").Object);

            //Act
            var result = await Sut.LoadByIdAsync(_rootId2);

            //Assert
            Assert.AreEqual("Bob", result.RootEntityName);
        }
    }
}