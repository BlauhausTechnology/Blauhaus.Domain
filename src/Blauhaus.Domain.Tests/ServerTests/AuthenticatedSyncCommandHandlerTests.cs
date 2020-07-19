using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Common.Errors;
using Blauhaus.Domain.Server.CommandHandlers;
using Blauhaus.Domain.Server.CommandHandlers.Sync;
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

            MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), _user, CancelToken))
                .ReturnsAsync(Result.Success(entityQuery));

            _entities = entityQuery.ToList();

            AddService(MockQueryLoader.Object);
        }

        public class LoadingOlderAndNewerEntites : AuthenticatedSyncCommandHandlerTests
        {
            [Test]
            public async Task SHOULD_fail()
            {
                //Arrange
                _command.OlderThan = 100;
                _command.NewerThan = 100;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);

                //Assert
                Assert.AreEqual(SyncErrors.InvalidSyncCommand.ToString(), queryResult.Error);
                MockAnalyticsService.VerifyTrace(SyncErrors.InvalidSyncCommand.Code, LogSeverity.Error);
            }
        }

        public class QueryableFail : AuthenticatedSyncCommandHandlerTests
        {
            [Test]
            public async Task WHEN_QueryLoader_fails_SHOULD_fail()
            {
                //Arrange
                MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), It.IsAny<TestAuthenticatedUser>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Failure<IQueryable<TestServerEntity>>("oh no"));

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);

                //Assert
                Assert.That(queryResult.Error, Is.EqualTo("oh no"));
            }
        }

        public class LoadingOlderEntities : AuthenticatedSyncCommandHandlerTests
        {
            [Test]
            public async Task SHOULD_return_entities_modified_before_given_ModifiedBeforeTicks_with_newest_first_and_oldest_excluded()
            {
                //Arrange
                _command.BatchSize = 3;
                _command.OlderThan = _entities[5].ModifiedAt.Ticks;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(12, result.TotalActiveEntityCount); 
                Assert.AreEqual(6, result.EntitiesToDownloadCount); 
                Assert.AreEqual(3, result.EntityBatch.Count); 
                result.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[6], 
                    _entities[7], 
                    _entities[8]
                }); 
                MockAnalyticsService.VerifyTrace("SyncCommand for older entities processed");
                MockAnalyticsService.VerifyTraceProperty("TotalActiveEntityCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntitiesToDownloadCount", 6);
                MockAnalyticsService.VerifyTraceProperty("EntityBatchCount", 3);
            }

            [Test]
            public async Task SHOULD_exclude_deleted_entities()
            {
                //Arrange
                _command.OlderThan = DateTime.UtcNow.Ticks;
                MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), _user, CancelToken))
                    .ReturnsAsync(Result.Success(new List<TestServerEntity>
                    {
                        new TestServerEntity(Guid.NewGuid(), EntityState.Active, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Draft, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Archived, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Deleted, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                    }.AsQueryable()));

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(3, result.EntityBatch.Count);
                Assert.AreEqual(3, result.TotalActiveEntityCount); 
                Assert.AreEqual(3, result.EntitiesToDownloadCount);
                Assert.IsNull(result.EntityBatch.FirstOrDefault(x => x.EntityState == EntityState.Deleted));
            }

            [Test]
            public async Task SHOULD_allow_paging_through_dataset()
            {
                //Act 1
                var result1 = await Sut.HandleAsync(new TestSyncCommand
                {
                    BatchSize = 4,
                    OlderThan = _entities[5].ModifiedAt.Ticks
                }, _user, CancelToken);
                Assert.AreEqual(4, result1.Value.EntityBatch.Count);
                Assert.AreEqual(12, result1.Value.TotalActiveEntityCount);
                Assert.AreEqual(6, result1.Value.EntitiesToDownloadCount);
                result1.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[6], 
                    _entities[7], 
                    _entities[8], 
                    _entities[9]
                }); 

                //Act 2
                var result2 = await Sut.HandleAsync(new TestSyncCommand
                {
                    BatchSize = 4,
                    OlderThan = result1.Value.EntityBatch.Last().ModifiedAt.Ticks
                }, _user, CancelToken);
                Assert.AreEqual(2, result2.Value.EntityBatch.Count);
                Assert.AreEqual(12, result2.Value.TotalActiveEntityCount);
                Assert.AreEqual(2, result2.Value.EntitiesToDownloadCount);
                result2.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[10], 
                    _entities[11], 
                }); 
            }

        }

        public class LoadingNewerEntities : AuthenticatedSyncCommandHandlerTests
        {
            
            [Test]
            public async Task SHOULD_return_entities_modified_after_given_ModifiedAfterTicks_with_oldest_first_and_newest_excluded()
            {
                //Arrange
                _command.BatchSize = 3;
                _command.NewerThan = _entities[5].ModifiedAt.Ticks;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(12, result.TotalActiveEntityCount); 
                Assert.AreEqual(5, result.EntitiesToDownloadCount); 
                Assert.AreEqual(3, result.EntityBatch.Count); 
                result.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[4], 
                    _entities[3], 
                    _entities[2]
                }); 
                MockAnalyticsService.VerifyTrace("SyncCommand for newer entities processed");
                MockAnalyticsService.VerifyTraceProperty("TotalActiveEntityCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntitiesToDownloadCount", 5);
                MockAnalyticsService.VerifyTraceProperty("EntityBatchCount", 3);
            }
            
            [Test]
            public async Task SHOULD_include_inactive_entities()
            {
                //Arrange
                _command.NewerThan = DateTime.UtcNow.AddDays(-11).Ticks;
                MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), _user, CancelToken))
                    .ReturnsAsync(Result.Success(new List<TestServerEntity>
                    {
                        new TestServerEntity(Guid.NewGuid(), EntityState.Deleted, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Active, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                    }.AsQueryable()));

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(2, result.EntityBatch.Count);
                Assert.AreEqual(1, result.TotalActiveEntityCount); 
                Assert.AreEqual(2, result.EntitiesToDownloadCount);
            }
            
            [Test]
            public async Task SHOULD_allow_paging_through_dataset()
            {
                //Act 1
                var result1 = await Sut.HandleAsync(new TestSyncCommand
                {
                    BatchSize = 4,
                    NewerThan = _entities[5].ModifiedAt.Ticks
                }, _user, CancelToken);
                Assert.AreEqual(4, result1.Value.EntityBatch.Count);
                Assert.AreEqual(12, result1.Value.TotalActiveEntityCount);
                Assert.AreEqual(5, result1.Value.EntitiesToDownloadCount);
                result1.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[4], 
                    _entities[3], 
                    _entities[2], 
                    _entities[1]
                }); 

                //Act 2
                var result2 = await Sut.HandleAsync(new TestSyncCommand
                {
                    BatchSize = 4,
                    NewerThan = result1.Value.EntityBatch.Last().ModifiedAt.Ticks
                }, _user, CancelToken);
                Assert.AreEqual(1, result2.Value.EntityBatch.Count);
                Assert.AreEqual(12, result2.Value.TotalActiveEntityCount);
                Assert.AreEqual(1, result2.Value.EntitiesToDownloadCount);
                result2.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[0], 
                }); 
            }
        }

        public class LoadingForFirstTime : AuthenticatedSyncCommandHandlerTests
        {
                       
            [Test]
            public async Task SHOULD_return_all_entites_with_newest_first_and_oldest_excluded()
            {
                //Arrange
                _command.BatchSize = 3;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(12, result.TotalActiveEntityCount); 
                Assert.AreEqual(12, result.EntitiesToDownloadCount); 
                Assert.AreEqual(3, result.EntityBatch.Count); 
                result.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[0], 
                    _entities[1], 
                    _entities[2]
                }); 
                MockAnalyticsService.VerifyTrace("SyncCommand for new device processed");
                MockAnalyticsService.VerifyTraceProperty("TotalActiveEntityCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntitiesToDownloadCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntityBatchCount", 3);
            }

            [Test]
            public async Task SHOULD_exclude_deleted_entities()
            {
                //Arrange
                MockQueryLoader.Mock.Setup(x => x.HandleAsync(It.IsAny<TestSyncCommand>(), _user, CancelToken))
                    .ReturnsAsync(Result.Success(new List<TestServerEntity>
                    {
                        new TestServerEntity(Guid.NewGuid(), EntityState.Active, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Draft, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Archived, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                        new TestServerEntity(Guid.NewGuid(), EntityState.Deleted, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
                    }.AsQueryable()));

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(3, result.EntityBatch.Count);
                Assert.AreEqual(3, result.TotalActiveEntityCount); 
                Assert.AreEqual(3, result.EntitiesToDownloadCount);
                Assert.IsNull(result.EntityBatch.FirstOrDefault(x => x.EntityState == EntityState.Deleted));
            }
                          
            [Test]
            public async Task SHOULD_allow_paging_through_dataset()
            {
                //Act 1
                var result1 = await Sut.HandleAsync(new TestSyncCommand
                {
                    BatchSize = 5
                }, _user, CancelToken);
                Assert.AreEqual(5, result1.Value.EntityBatch.Count);
                Assert.AreEqual(12, result1.Value.TotalActiveEntityCount);
                Assert.AreEqual(12, result1.Value.EntitiesToDownloadCount);
                result1.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
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
                    OlderThan = result1.Value.EntityBatch.Last().ModifiedAt.Ticks
                }, _user, CancelToken);
                Assert.AreEqual(5, result2.Value.EntityBatch.Count);
                Assert.AreEqual(12, result2.Value.TotalActiveEntityCount);
                Assert.AreEqual(7, result2.Value.EntitiesToDownloadCount);
                result2.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
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
                    OlderThan = result2.Value.EntityBatch.Last().ModifiedAt.Ticks
                }, _user, CancelToken);
                Assert.AreEqual(2, result3.Value.EntityBatch.Count); 
                Assert.AreEqual(12, result3.Value.TotalActiveEntityCount);
                Assert.AreEqual(2, result3.Value.EntitiesToDownloadCount);
                result3.Value.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[10], 
                    _entities[11]
                }); 
            }

        }

        public class LoadingSingleEntity : AuthenticatedSyncCommandHandlerTests
        {
            [Test]
            public async Task IF_NewerThan_is_not_given_SHOULD_return_Entity_matching_Id()
            {
                //Arrange
                _command.Id = _entities[4].Id;
                _command.NewerThan = null;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(12, result.TotalActiveEntityCount); 
                Assert.AreEqual(1, result.EntitiesToDownloadCount); 
                Assert.AreEqual(1, result.EntityBatch.Count); 
                result.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[4]
                }); 
                MockAnalyticsService.VerifyTrace("SyncCommand for single entity processed");
                MockAnalyticsService.VerifyTraceProperty("TotalActiveEntityCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntitiesToDownloadCount", 1);
                MockAnalyticsService.VerifyTraceProperty("EntityBatchCount", 1);
            }

            [Test]
            public async Task IF_NewerThan_is_given_and_entity_has_not_been_modified_since_SHOULD_not_return_it()
            {
                //Arrange
                _command.Id = _entities[4].Id;
                _command.NewerThan = _entities[4].ModifiedAtTicks;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(12, result.TotalActiveEntityCount); 
                Assert.AreEqual(0, result.EntitiesToDownloadCount); 
                Assert.AreEqual(0, result.EntityBatch.Count);  
                MockAnalyticsService.VerifyTrace("SyncCommand for single entity processed");
                MockAnalyticsService.VerifyTraceProperty("TotalActiveEntityCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntitiesToDownloadCount", 0);
                MockAnalyticsService.VerifyTraceProperty("EntityBatchCount", 0);
            }

            [Test]
            public async Task IF_NewerThan_is_given_and_entity_has_been_modified_since_SHOULD_return_it()
            {
                //Arrange
                _command.Id = _entities[4].Id;
                _command.NewerThan = _entities[4].ModifiedAtTicks - 1;

                //Act
                var queryResult = await Sut.HandleAsync(_command, _user, CancelToken);
                var result = queryResult.Value;

                //Assert
                Assert.AreEqual(12, result.TotalActiveEntityCount); 
                Assert.AreEqual(1, result.EntitiesToDownloadCount); 
                Assert.AreEqual(1, result.EntityBatch.Count); 
                result.EntityBatch.VerifyEntities(new List<TestServerEntity>
                {
                    _entities[4]
                }); 
                MockAnalyticsService.VerifyTrace("SyncCommand for single entity processed");
                MockAnalyticsService.VerifyTraceProperty("TotalActiveEntityCount", 12);
                MockAnalyticsService.VerifyTraceProperty("EntitiesToDownloadCount", 1);
                MockAnalyticsService.VerifyTraceProperty("EntityBatchCount", 1);
            }
        }
         
    }
}