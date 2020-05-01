﻿using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests
{
    public class VoidCommandClientHandlerTests : BaseServiceTest<VoidCommandClientHandler<TestCommandDto, TestCommand>>
    {
        private TestCommand _command;
        private TestCommandDto _commandDto;

        private AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
        private MockBuilder<IVoidCommandHandler<TestCommandDto>> MockDtoCommandHandler => AddMock<IVoidCommandHandler<TestCommandDto>>().Invoke();
        private MockBuilder<ICommandConverter<TestCommandDto, TestCommand>> MockCommandConverter => AddMock<ICommandConverter<TestCommandDto, TestCommand>>().Invoke();

        [SetUp]
        public void Setup()
        {
            Cleanup();
            
            _command = new TestCommand();
            _commandDto = new TestCommandDto{Name = "Converted Name"};

            MockCommandConverter.Mock.Setup(x => x.Convert(_command)).Returns(_commandDto);
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancellationToken)).ReturnsAsync(Result.Success());

            AddService(MockAnalyticsService.Object);
            AddService(MockCommandConverter.Object);
            AddService(MockDtoCommandHandler.Object);
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
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancellationToken)).ReturnsAsync(Result.Failure("oops"));
            
            //Act
            var result = await Sut.HandleAsync(_command, CancellationToken);

            //Assert
            Assert.AreEqual("oops", result.Error);
        }

        [Test]
        public async Task IF_handler_succeeds_SHOULD_return_success()
        {
            //Act
            var result = await Sut.HandleAsync(_command, CancellationToken);

            //Assert
            Assert.IsTrue(result.IsSuccess);
        }
    }
}