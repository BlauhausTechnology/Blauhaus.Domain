namespace Blauhaus.Domain.Client.CommandHandlers
{
    public interface ICommandConverter<TCommandDto, TCommand>
    {
        TCommandDto Convert(TCommand command);
    }
}