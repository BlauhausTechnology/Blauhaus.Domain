using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Responses;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Client.CommandHandlers
{
    public class EntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand> : ICommandHandler<TModel, TCommand> 
        where TCommand: notnull
        where TCommandDto: notnull
        where TModel : class, IClientEntity 
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ICommandConverter<TCommandDto, TCommand> _converter;
        private readonly ICommandHandler<TModelDto, TCommandDto> _dtoCommandHandler;
        private readonly IClientRepository<TModel, TModelDto> _repository;

        public EntityCommandClientHandler(
            IAnalyticsService analyticsService,
            ICommandConverter<TCommandDto, TCommand> converter,
            ICommandHandler<TModelDto, TCommandDto> dtoCommandHandler, 
            IClientRepository<TModel, TModelDto> repository)
        {
            _analyticsService = analyticsService;
            _converter = converter;
            _dtoCommandHandler = dtoCommandHandler;
            _repository = repository;
        }

        //todo handle offline case and return error?

        public async Task<Response<TModel>> HandleAsync(TCommand command, CancellationToken token)
        {
            _analyticsService.TraceVerbose(this, $"{typeof(TCommand).Name} Handler started", command.ToObjectDictionary("Command"));

            var commandDto = _converter.Convert(command);
            var dtoResult = await _dtoCommandHandler.HandleAsync(commandDto, token);
            if (dtoResult.IsFailure)
            {
                return Response.Failure<TModel>(dtoResult.Error);
            }

            var model = await _repository.SaveDtoAsync(dtoResult.Value);

            _analyticsService.TraceVerbose(this, $"{typeof(TCommand).Name} Handler succeeded");

            return Response.Success(model);
        }
    }
}