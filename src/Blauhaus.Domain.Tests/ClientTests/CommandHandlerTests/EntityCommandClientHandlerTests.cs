using System;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.CommandHandlerTests
{
    public class EntityCommandClientHandlerTests : BaseDomainTest<TestClientEntityCommandHandler>
    {
        private TestCommand _command;
        private TestCommandDto _commandDto;
        private TestModelDto _modelDto;
        private TestModel _model;

        private MockBuilder<ICommandHandler<TestModelDto, TestCommandDto>> MockDtoCommandHandler => AddMock<ICommandHandler<TestModelDto, TestCommandDto>>().Invoke();
        private ClientRepositoryMockBuilder<TestModel, TestModelDto> MockClientRepository 
            => AddMock<ClientRepositoryMockBuilder<TestModel, TestModelDto>, IClientRepository<TestModel, TestModelDto>>().Invoke();
        
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
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto)).ReturnsAsync(Response.Success(_modelDto));
            MockClientRepository.Where_SaveDtoAsync_returns(_model);

            AddService(MockCommandConverter.Object);
            AddService(MockDtoCommandHandler.Object);
            AddService(MockClientRepository.Object);
        }

        [Test]
        public async Task SHOULD_trace_start_and_success()
        {
            //Act
            await Sut.HandleAsync(_command);

            //Assert
            MockAnalyticsService.VerifyTrace("TestCommand Handler started");
            MockAnalyticsService.VerifyTraceProperty("Command", _command);
            MockAnalyticsService.VerifyTrace("TestCommand Handler succeeded");
        }

        [Test]
        public async Task SHOULD_convert_Command_to_CommandDto_and_handle()
        {
            //Act
            await Sut.HandleAsync(_command);

            //Assert
            MockDtoCommandHandler.Mock.Verify(x => x.HandleAsync(_commandDto));
        }

        [Test]
        public async Task IF_handler_fails_SHOULD_return_failure()
        {
            //Arrange
            MockDtoCommandHandler.Mock.Setup(x => x.HandleAsync(_commandDto)).ReturnsAsync(Response.Failure<TestModelDto>(Error.Create("oops")));
            
            //Act
            var result = await Sut.HandleAsync(_command);

            //Assert
            Assert.AreEqual("oops", result.Error.Description);
        }

        [Test]
        public async Task IF_handler_succeeds_SHOULD_save_and_return_Dto()
        {
            //Act
            var result = await Sut.HandleAsync(_command);

            //Assert
            MockClientRepository.Mock.Verify(x => x.SaveDtoAsync(_modelDto));
            Assert.AreEqual(_model, result.Value);
        }
         
    }
}