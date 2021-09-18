using Blauhaus.Domain.Client.CommandHandlers;

namespace Blauhaus.Domain.Tests.TestObjects.Client
{
    public class TestCommandConverter : ICommandConverter<TestCommandDto, TestCommand>
    {
        public TestCommandDto Convert(TestCommand command)
        {
            return new TestCommandDto
            {
                Name = command.Name
            };
        }
    }
}