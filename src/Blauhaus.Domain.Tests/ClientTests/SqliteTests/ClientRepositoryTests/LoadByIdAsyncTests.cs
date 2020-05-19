using System;
using System.Collections.Generic;
using System.Linq;
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
    public class LoadByIdAsyncTests : BaseSqliteTest<ClientRepository<ITestModel, ITestDto, TestRootEntity>>
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
        public async Task SHOULD_construct_model_using_RootEntity()
        {
            //Arrange
            MockClientEntityConverter.Where_LoadChildEntities_returns(new List<ISyncClientEntity>
            {
                new TestChildEntity {ChildName = "Bobby", ModifiedAtTicks = 13}
            });

            //Act
            await Sut.LoadByIdAsync(_rootId2);

            //Assert
            MockClientEntityConverter.Mock.Verify(x => x.ConstructModel(
                It.Is<TestRootEntity>(y => y.Id == _rootId2), It.IsAny<List<ISyncClientEntity>>()));
            MockClientEntityConverter.Mock.Verify(x => x.ConstructModel(
                It.IsAny<TestRootEntity>(), It.Is<List<ISyncClientEntity>>(y => y.First().ModifiedAtTicks == 13)));
        }


        [Test]
        public async Task SHOULD_return_constructed_model()
        {
            //Arrange
            MockClientEntityConverter.Where_ConstructModel_returns(new MockBuilder<ITestModel>()
                .With(x => x.RootEntityName, "Bob").Object);

            //Act
            var result = await Sut.LoadByIdAsync(_rootId2);

            //Assert
            Assert.AreEqual("Bob", result.RootEntityName);
        }
    }
}