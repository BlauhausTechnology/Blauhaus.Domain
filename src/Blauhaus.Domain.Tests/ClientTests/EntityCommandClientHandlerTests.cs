using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests
{
    public class EntityCommandClientHandlerTests : BaseDomainTest<TestClientEntityCommandHandler>
    {
        private TestCommand _command;
        private TestCommandDto _commandDto;
        private TestModelDto _modelDto;
        private TestModel _model;

        private MockBuilder<ICommandHandler<TestModelDto, TestCommandDto>> MockDtoCommandHandler => AddMock<ICommandHandler<TestModelDto, TestCommandDto>>().Invoke();
        private MockBuilder<IClientRepository<TestModel, TestModelDto>> MockClientRepository => AddMock<IClientRepository<TestModel, TestModelDto>>().Invoke();
        private MockBuilder<ICommandConverter<TestCommandDto, TestCommand>> MockCommandConverter => AddMock<ICommandConverter<TestCommandDto, TestCommand>>().Invoke();

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            _command = new TestCommand();
            _commandDto = new TestCommandDto{Name = "Converted Name"};
            _modelDto = new TestModelDto{Name = "Model Dto"};
            _model = new TestModel(Guid.NewGuid(), EntityState.Active, 1000, "Bob");

            MockCommandConverter.Mock.Setup(x => x.Convert(_command)).Returns(_commandDto);
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancellationToken)).ReturnsAsync(Result.Success(_modelDto));
            MockClientRepository.Mock.Setup(x => x.SaveDtoAsync(_modelDto)).ReturnsAsync(_model);

            AddService(MockCommandConverter.Object);
            AddService(MockDtoCommandHandler.Object);
            AddService(MockClientRepository.Object);
        }

        [Test]
        public async Task SHOULD_trace_start_and_success()
        {
            //Act
            await Sut.HandleAsync(_command, CancellationToken);

            //Assert
            MockAnalyticsService.VerifyTrace("TestCommand Handler started");
            MockAnalyticsService.VerifyTraceProperty("Command", _command);
            MockAnalyticsService.VerifyTrace("TestCommand Handler succeeded");
        }

        [Test]
        public async Task SHOULD_convert_Command_to_CommandDto_and_handle()
        {
            //Act
            await Sut.HandleAsync(_command, CancellationToken);

            //Assert
            MockDtoCommandHandler.Mock.Verify(x => x.HandleAsync(_commandDto, CancellationToken));
        }

        [Test]
        public async Task IF_handler_fails_SHOULD_return_failure()
        {
            //Arrange
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancellationToken)).ReturnsAsync(Result.Failure<TestModelDto>("oops"));
            
            //Act
            var result = await Sut.HandleAsync(_command, CancellationToken);

            //Assert
            Assert.AreEqual("oops", result.Error);
        }

        [Test]
        public async Task IF_handler_succeeds_SHOULD_save_and_return_Dto()
        {
            //Act
            var result = await Sut.HandleAsync(_command, CancellationToken);

            //Assert
            MockClientRepository.Mock.Verify(x => x.SaveDtoAsync(_modelDto));
            Assert.AreEqual(_model, result.Value);
        }
         
    }
}