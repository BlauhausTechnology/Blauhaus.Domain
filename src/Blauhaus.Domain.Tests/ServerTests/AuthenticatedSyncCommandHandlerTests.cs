using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Server.CommandHandlers;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.Extensions;
using Blauhaus.Domain.Tests.ServerTests.TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ServerTests
{
    public class AuthenticatedSyncCommandHandlerTests : BaseDomainTest<AuthenticatedSyncCommandHandler<TestServerEntity, TestSyncCommand, TestAuthenticatedUser>>
    {
        private TestSyncCommand _command = new TestSyncCommand();
        private List<TestServerEntity> _entities = new List<TestServerEntity>();
        private TestAuthenticatedUser _user = new TestAuthenticatedUser();

        private MockBuilder<IAuthenticatedSyncQueryLoader<TestServerEntity, TestSyncCommand, TestAuthenticatedUser>> MockQueryLoader => AddMock<IAuthenticatedSyncQueryLoader<TestServerEntity, TestSyncCommand, TestAuthenticatedUser>>().Invoke();

        public override void Setup()
        {
            base.Setup();

            _command = new TestSyncCommand{RandomOtherFilterParameter = 10};
            _user = new TestAuthenticatedUser {UserId = Guid.NewGuid()};
            var entityQuery = TestServerEntity.GenerateList(12);

            MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), _user, CancellationToken))
                .ReturnsAsync(Result.Success(entityQuery));

            _entities = entityQuery.ToList();

            AddService(MockQueryLoader.Object);
        }

        [Test]
        public async Task IF_batch_size_is_overriden_SHOULD_return_most_recent_entities()
        {
            //Arrange
            _command.BatchSize = 3;

            //Act
            var queryResult = await Sut.HandleAsync(_command, _user, CancellationToken);
            var result = queryResult.Value;

            //Assert
            Assert.AreEqual(3, result.Entities.Count);
            Assert.AreEqual(12, result.TotalCount);
            result.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[0],
                _entities[1],
                _entities[2]
            });
        }

        
        [Test]
        public async Task IF_ModifiedAfter_is_specified_and_batch_size_is_less_than_result_set_SHOULD_return_only_most_recently_modified()
        {
            //Arrange
            _command.BatchSize = 4;
            _command.ModifiedAfter = _entities[8].ModifiedAt.Ticks;

            //Act
            var queryResult = await Sut.HandleAsync(_command, _user, CancellationToken);
            var result = queryResult.Value;

            //Assert
            Assert.AreEqual(4, result.Entities.Count);
            Assert.AreEqual(12, result.TotalCount); 
            result.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[0], 
                _entities[1], 
                _entities[2],
                _entities[3]
            }); 
        }

        [Test]
        public async Task IF_ModifiedAfter_is_specified_and_batch_size_is_greater_than_result_set_SHOULD_return_only_most_recently_modified()
        {
            //Arrange
            _command.BatchSize = 7;
            _command.ModifiedAfter = _entities[4].ModifiedAt.Ticks;

            //Act
            var queryResult = await Sut.HandleAsync(_command, _user, CancellationToken);
            var result = queryResult.Value;

            //Assert
            Assert.AreEqual(12, result.TotalCount); 
            Assert.AreEqual(4, result.Entities.Count);
            result.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[0], 
                _entities[1], 
                _entities[2],
                _entities[3]
            }); 
        }

        [Test]
        public async Task IF_ModifiedBefore_is_specified_SHOULD_return_only_entities_modified_before()
        {
            //Arrange
            _command.BatchSize = 3;
            _command.ModifiedBefore = _entities[5].ModifiedAt.Ticks;

            //Act
            var queryResult = await Sut.HandleAsync(_command, _user, CancellationToken);
            var result = queryResult.Value;

            //Assert
            Assert.AreEqual(12, result.TotalCount); 
            Assert.AreEqual(3, result.Entities.Count); 
            result.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[6], 
                _entities[7], 
                _entities[8]
            }); 
        }

        [Test]
        public async Task IF_ModifiedBefore_is_specified_and_batch_size_is_smaller_than_result_set_SHOULD_remove_older_items()
        {
            //Arrange
            _command.BatchSize = 2;
            _command.ModifiedBefore = _entities[5].ModifiedAt.Ticks;

            //Act
            var queryResult = await Sut.HandleAsync(_command, _user, CancellationToken);
            var result = queryResult.Value;

            //Assert
            Assert.AreEqual(12, result.TotalCount); 
            Assert.AreEqual(2, result.Entities.Count); 
            result.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[6], 
                _entities[7] 
            }); 
        }

        [Test]
        public async Task IF_ModifiedBefore_and_ModifiedAfter_are_specified_SHOULD_return_entities_matching_either_not_both()
        {
            //Arrange
            _command.ModifiedAfter = _entities[3].ModifiedAt.Ticks;
            _command.ModifiedBefore = _entities[8].ModifiedAt.Ticks;

            //Act
            var queryResult = await Sut.HandleAsync(_command, _user, CancellationToken);
            var result = queryResult.Value;

            //Assert
            Assert.AreEqual(6, result.Entities.Count);
            Assert.AreEqual(12, result.TotalCount); 
            result.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[0], 
                _entities[1], 
                _entities[2], 
                _entities[9], 
                _entities[10], 
                _entities[11] 
            }); 
        }

        [Test]
        public async Task SHOULD_allow_paging_through_dataset()
        {
            //Act 1
            var result1 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5
            }, _user, CancellationToken);
            Assert.AreEqual(5, result1.Value.Entities.Count);
            Assert.AreEqual(12, result1.Value.TotalCount);
            result1.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[0], 
                _entities[1], 
                _entities[2], 
                _entities[3], 
                _entities[4] 
            }); 

            //Act 2
            var result2 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5,
                ModifiedBefore = result1.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(5, result2.Value.Entities.Count);
            Assert.AreEqual(12, result2.Value.TotalCount);
            result2.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[5], 
                _entities[6], 
                _entities[7], 
                _entities[8], 
                _entities[9] 
            }); 

            //Act 3
            var result3 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5,
                ModifiedBefore = result2.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(2, result3.Value.Entities.Count); 
            Assert.AreEqual(12, result3.Value.TotalCount);
            result3.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[10], 
                _entities[11]
            }); 

        }

        [Test]
        public async Task IF_entity_is_downloaded_during_sync_then_modified_before_sync_completes_SHOULD_ignore_until_next_sync()
        {
            //Act 1
            var result1 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5
            }, _user, CancellationToken);
            var result2 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5,
                ModifiedBefore = result1.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(5, result1.Value.Entities.Count);
            Assert.AreEqual(12, result1.Value.TotalCount);
            result1.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[0], 
                _entities[1], 
                _entities[2], 
                _entities[3], 
                _entities[4] 
            }); 

            //Act 2
            Assert.AreEqual(5, result2.Value.Entities.Count);
            Assert.AreEqual(12, result2.Value.TotalCount);
            result2.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[5], 
                _entities[6], 
                _entities[7], 
                _entities[8], 
                _entities[9] 
            }); 
            var userToChange = _entities.First(x => x.Id == result2.Value.Entities[2].Id);
            userToChange.ModifiedAt = DateTime.UtcNow;

            //Act 3
            var result3 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5,
                ModifiedBefore = result2.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(2, result3.Value.Entities.Count); 
            Assert.AreEqual(12, result3.Value.TotalCount);
            result3.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[10], 
                _entities[11]
            }); 
            
            //Act 4
            var result4 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5,
                ModifiedAfter = result1.Value.Entities.First().ModifiedAt.Ticks,
                ModifiedBefore = result3.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(1, result4.Value.Entities.Count); 
            Assert.AreEqual(12, result4.Value.TotalCount);
            result4.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                userToChange
            }); 
        }
        
        [Test]
        public async Task IF_sync_is_partially_completed_SHOULD_get_missing_and_new_and_modified_entites_next_time()
        {
            //Act 1
            var result1 = await Sut.HandleAsync(new TestSyncCommand
            {
                BatchSize = 5
            }, _user, CancellationToken);

            //change entity
            var modifiedUser = _entities.First(x => x.Id == result1.Value.Entities[2].Id);
            modifiedUser.ModifiedAt = DateTime.UtcNow;

            //add entity
            var newUser = new TestServerEntity(Guid.NewGuid(), EntityState.Active, DateTime.UtcNow, DateTime.UtcNow);
            MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), It.IsAny<TestAuthenticatedUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<TestServerEntity>(_entities) { newUser }.AsQueryable()));

            //new sync
            var result2 = await Sut.HandleAsync(new TestSyncCommand()
            {
                BatchSize = 5,
                ModifiedAfter = result1.Value.Entities.First().ModifiedAt.Ticks,
                ModifiedBefore = result1.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(5, result2.Value.Entities.Count);
            Assert.AreEqual(13, result2.Value.TotalCount);
            result2.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                newUser,
                modifiedUser,
                _entities[5], 
                _entities[6], 
                _entities[7] 
            }); 

            //Act 3
            var result3 = await Sut.HandleAsync(new TestSyncCommand()
            {
                BatchSize = 5,
                ModifiedBefore = result2.Value.Entities.Last().ModifiedAt.Ticks
            }, _user, CancellationToken);
            Assert.AreEqual(4, result3.Value.Entities.Count); 
            Assert.AreEqual(13, result3.Value.TotalCount);
            result3.Value.Entities.VerifyEntities(new List<TestServerEntity>
            {
                _entities[8], 
                _entities[9], 
                _entities[10], 
                _entities[11]
            }); 

        }
    }
}