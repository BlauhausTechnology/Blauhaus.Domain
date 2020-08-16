using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Abstractions.CommandHandlers;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestClientEntityCommandHandler : EntityCommandClientHandler<TestModel, TestModelDto, TestCommandDto, TestCommand>
    {
        public TestClientEntityCommandHandler(
            IAnalyticsService analyticsService, 
            ICommandConverter<TestCommandDto, TestCommand> converter, 
            ICommandHandler<TestModelDto, TestCommandDto> dtoCommandHandler, 
            IClientRepository<TestModel, TestModelDto> repository) 
                : base(analyticsService, converter, dtoCommandHandler, repository)
        {
        }
    }
}