using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._Base;
using Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests.ClientRepositoryTests
{
    public class LoadByIdAsyncTests : BaseSqliteTest<ClientRepository<ITestModel, TestRootEntity, ITestDto>>
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
        public async Task SHOULD_load_correct_entity()
        {
            //Act
            await Sut.LoadByIdAsync(_rootId2);

            //Assert
            MockClientRepositoryHelper.Mock.Verify(x => x.ConstructModelFromRootEntity(It.Is<TestRootEntity>(y => y.Id == _rootId2), It.IsAny<SQLiteConnection>()));
        }

        [Test]
        public async Task SHOULD_return_constructed_model()
        {
            //Arrange
            MockClientRepositoryHelper.Where_ConstructModelFromRootEntity_returns(new MockBuilder<ITestModel>()
                .With(x => x.RootEntityName, "Bob").Object);

            //Act
            var result = await Sut.LoadByIdAsync(_rootId2);

            //Assert
            Assert.AreEqual("Bob", result.RootEntityName);
        }
    }
}