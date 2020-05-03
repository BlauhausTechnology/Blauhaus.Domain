using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Client.CommandHandlers.Sync
{
    public class SyncCommandClientHandler<TModel, TModelDto, TSyncCommandDto, TSyncCommand> : ICommandHandler<SyncResult<TModel>, TSyncCommand> 
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
        where TSyncCommandDto : notnull
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ICommandConverter<TSyncCommandDto, TSyncCommand> _converter;
        private readonly ICommandHandler<DtoSyncResult<TModelDto>, TSyncCommandDto> _dtoCommandHandler;
        private readonly ISyncClientRepository<TModel, TModelDto, TSyncCommand> _repository;

        public SyncCommandClientHandler(
            IAnalyticsService analyticsService,
            ICommandConverter<TSyncCommandDto, TSyncCommand> converter,
            ICommandHandler<DtoSyncResult<TModelDto>, TSyncCommandDto> dtoCommandHandler,
            ISyncClientRepository<TModel, TModelDto, TSyncCommand> repository)
        {
            _analyticsService = analyticsService;
            _converter = converter;
            _dtoCommandHandler = dtoCommandHandler;
            _repository = repository;
        }


        public async Task<Result<SyncResult<TModel>>> HandleAsync(TSyncCommand command, CancellationToken token)
        {
            var mess = $"{typeof(TSyncCommand).Name} handler for {typeof(TModel).Name} started";
            _analyticsService.TraceVerbose(this, $"{typeof(TSyncCommand).Name} handler for {typeof(TModel).Name} started", command.ToObjectDictionary("Command"));

            var commandDto = _converter.Convert(command);
            var dtoResult = await _dtoCommandHandler.HandleAsync(commandDto, token);
            if (dtoResult.IsFailure)
            {
                return Result.Failure<SyncResult<TModel>>(dtoResult.Error);
            }

            var models = await _repository.SaveSyncedDtosAsync(dtoResult.Value.Dtos);

            _analyticsService.TraceVerbose(this,  $"{typeof(TSyncCommand).Name} handler for {typeof(TModel).Name} succeeded");

            return Result.Success(new SyncResult<TModel>
            {
                Entities = (List<TModel>) models,
                TotalEntityCount = dtoResult.Value.TotalEntityCount,
                ModifiedEntityCount = dtoResult.Value.ModifiedEntityCount
            });
        }
    }
}