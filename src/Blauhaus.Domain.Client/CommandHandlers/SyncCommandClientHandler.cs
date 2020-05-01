using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Client.CommandHandlers
{
    public class SyncCommandClientHandler<TModel, TModelDto, TSyncCommandDto, TSyncCommand> : ICommandHandler<SyncResult<TModel>, TSyncCommand> 
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
        where TSyncCommandDto : notnull
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ICommandConverter<TSyncCommandDto, TSyncCommand> _converter;
        private readonly ICommandHandler<TModelDto, TSyncCommandDto> _dtoCommandHandler;
        private readonly IClientRepository<TModel, TModelDto> _repository;

        public SyncCommandClientHandler(
            IAnalyticsService analyticsService,
            ICommandConverter<TSyncCommandDto, TSyncCommand> converter,
            ICommandHandler<TModelDto, TSyncCommandDto> dtoCommandHandler,
            IClientRepository<TModel, TModelDto> repository)
        {
            _analyticsService = analyticsService;
            _converter = converter;
            _dtoCommandHandler = dtoCommandHandler;
            _repository = repository;
        }


        public async Task<Result<SyncResult<TModel>>> HandleAsync(TSyncCommand command, CancellationToken token)
        {
          
            _analyticsService.TraceVerbose(this, $"{typeof(TSyncCommand).Name} Sync handler started", command.ToObjectDictionary("Command"));

            var commandDto = _converter.Convert(command);
            var dtoResult = await _dtoCommandHandler.HandleAsync(commandDto, token);
            if (dtoResult.IsFailure)
            {
                return Result.Failure<SyncResult<TModel>>(dtoResult.Error);
            }

            //var model = await _repository.SaveDtoAsync(dtoResult.Value);

            _analyticsService.TraceVerbose(this, $"{typeof(TSyncCommand).Name} Sync handler succeeded");

            return Result.Success(new SyncResult<TModel>());
        }
    }
}