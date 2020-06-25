using System.Threading.Tasks;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Tests._Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.CommandHandlerTests
{
    public class VoidCommandClientHandlerTests : BaseDomainTest<VoidCommandClientHandler<TestCommandDto, TestCommand>>
    {
        private TestCommand _command;
        private TestCommandDto _commandDto;

        private MockBuilder<IVoidCommandHandler<TestCommandDto>> MockDtoCommandHandler => AddMock<IVoidCommandHandler<TestCommandDto>>().Invoke();
        private MockBuilder<ICommandConverter<TestCommandDto, TestCommand>> MockCommandConverter => AddMock<ICommandConverter<TestCommandDto, TestCommand>>().Invoke();

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            _command = new TestCommand();
            _commandDto = new TestCommandDto{Name = "Converted Name"};

            MockCommandConverter.Mock.Setup(x => x.Convert(_command)).Returns(_commandDto);
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancelToken)).ReturnsAsync(Result.Success());

            AddService(MockCommandConverter.Object);
            AddService(MockDtoCommandHandler.Object);
        }

        [Test]
        public async Task SHOULD_trace_start_and_success()
        {
            //Act
            await Sut.HandleAsync(_command, CancelToken);

            //Assert
            MockAnalyticsService.VerifyTrace("TestCommand Handler started");
            MockAnalyticsService.VerifyTraceProperty("Command", _command);
            MockAnalyticsService.VerifyTrace("TestCommand Handler succeeded");
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
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto, CancelToken)).ReturnsAsync(Result.Failure("oops"));
            
            //Act
            var result = await Sut.HandleAsync(_command, CancelToken);

            //Assert
            Assert.AreEqual("oops", result.Error);
        }

        [Test]
        public async Task IF_handler_succeeds_SHOULD_return_success()
        {
            //Act
            var result = await Sut.HandleAsync(_command, CancelToken);

            //Assert
            Assert.IsTrue(result.IsSuccess);
        }
    }
}