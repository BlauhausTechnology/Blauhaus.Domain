﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync.CommandHandler;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Blauhaus.Domain.Tests.ClientTests.CommandHandlerTests
{
    public class SyncCommandClientHandlerTests : BaseDomainTest<SyncCommandClientHandler<TestModel, TestModelDto, TestSyncCommandDto, TestSyncCommand>>
    {
        private TestSyncCommand _command;
        private TestSyncCommandDto _commandDto;
        private TestModelDto _modelDto;
        private TestModel _model;
        private DtoSyncResult<TestModelDto> _dtoSyncResult;

        private MockBuilder<ICommandConverter<TestSyncCommandDto, TestSyncCommand>> MockCommandConverter 
            => AddMock<ICommandConverter<TestSyncCommandDto, TestSyncCommand>>().Invoke();

        private MockBuilder<ICommandHandler<DtoSyncResult<TestModelDto>, TestSyncCommandDto>> MockDtoCommandHandler
            => AddMock<ICommandHandler<DtoSyncResult<TestModelDto>, TestSyncCommandDto>>().Invoke();

        private BaseSyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand> MockBaseSyncClientRepository 
            => AddMock<BaseSyncClientRepositoryMockBuilder<TestModel, TestModelDto, TestSyncCommand>, ISyncClientRepository<TestModel, TestModelDto, TestSyncCommand>>().Invoke();


        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            _command = new TestSyncCommand();
            _commandDto = new TestSyncCommandDto{};
            _modelDto = new TestModelDto{Name = "Model Dto"};
            _model = new TestModel(Guid.NewGuid(), EntityState.Active, 1000, "Bob");
            _dtoSyncResult = new DtoSyncResult<TestModelDto>
            {
                Dtos = new List<TestModelDto>{_modelDto},
                ModifiedEntityCount = 1,
                TotalEntityCount = 2
            };

            MockCommandConverter.Mock.Setup(x => x.Convert(_command)).Returns(_commandDto);
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancelToken))
                .ReturnsAsync(Result.Success(_dtoSyncResult));
            MockBaseSyncClientRepository.Where_SaveSyncedDtosAsync_returns(new List<TestModel> {_model});

            AddService(MockCommandConverter.Object);
            AddService(MockDtoCommandHandler.Object);
            AddService(MockBaseSyncClientRepository.Object);
        }

        [Test]
        public async Task SHOULD_trace_start_and_success()
        {
            //Act
            await Sut.HandleAsync(_command, CancelToken);

            //Assert
            MockAnalyticsService.VerifyTrace("TestSyncCommand handler for TestModel started");
            MockAnalyticsService.VerifyTraceProperty("Command", _command);
            MockAnalyticsService.VerifyTrace("TestSyncCommand handler for TestModel succeeded");
        }

        [Test]
        public async Task SHOULD_convert_Command_to_CommandDto_and_handle()
        {
            //Act
            await Sut.HandleAsync(_command, CancelToken);

            //Assert
            MockDtoCommandHandler.Mock.Verify(x => x.HandleAsync(_commandDto, CancelToken));
        }

        [Test]
        public async Task IF_handler_fails_SHOULD_return_failure()
        {
            //Arrange
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancelToken)).ReturnsAsync(Result.Failure<DtoSyncResult<TestModelDto>>("oops"));

            //Act
            var result = await Sut.HandleAsync(_command, CancelToken);

            //Assert
            Assert.AreEqual("oops", result.Error);
        }

        [Test]
        public async Task IF_handler_succeeds_SHOULD_save_and_return_Dtos()
        {
            //Act
            var result = await Sut.HandleAsync(_command, CancelToken);

            //Assert
            MockBaseSyncClientRepository.Mock.Verify(x => x.SaveSyncedDtosAsync(It.Is<IReadOnlyList<TestModelDto>>(y => 
                y[0] == _modelDto)));
            Assert.AreEqual(_model, result.Value.EntityBatch.First());
            Assert.AreEqual(1, result.Value.EntitiesToDownloadCount);
            Assert.AreEqual(2, result.Value.TotalActiveEntityCount);
        }

    }
}