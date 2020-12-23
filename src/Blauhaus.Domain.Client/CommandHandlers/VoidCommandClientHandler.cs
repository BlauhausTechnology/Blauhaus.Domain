using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.CommandHandlers
{
    public class VoidCommandClientHandler<TCommandDto, TCommand> : IVoidCommandHandler<TCommand>
    {
        private IAnalyticsService _analyticsService;
        private ICommandConverter<TCommandDto, TCommand> _converter;
        private IVoidCommandHandler<TCommandDto> _dtoCommandHandler;

        public VoidCommandClientHandler(IAnalyticsService analyticsService,
            ICommandConverter<TCommandDto, TCommand> converter,
            IVoidCommandHandler<TCommandDto> dtoCommandHandler)
        {
            _analyticsService = analyticsService;
            _converter = converter;
            _dtoCommandHandler = dtoCommandHandler;
        }

        public async Task<Response> HandleAsync(TCommand command)
        {
            _analyticsService.TraceVerbose(this, $"{typeof(TCommand).Name} Handler started", command.ToObjectDictionary("Command"));

            var commandDto = _converter.Convert(command);
            var dtoResult = await _dtoCommandHandler.HandleAsync(commandDto);
            if (dtoResult.IsFailure)
            {
                return Response.Failure(dtoResult.Error);
            }

            _analyticsService.TraceVerbose(this, $"{typeof(TCommand).Name} Handler succeeded");

            return Response.Success();
        }
    }
}