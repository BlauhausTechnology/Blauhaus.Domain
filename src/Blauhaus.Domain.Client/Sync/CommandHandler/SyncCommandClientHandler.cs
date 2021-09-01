using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync.CommandHandler
{
    public class SyncCommandClientHandler<TModel, TModelDto, TSyncCommandDto, TSyncCommand> : ICommandHandler<SyncResult<TModel>, TSyncCommand> 
        where TModel : class, IClientEntity<Guid>
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


        public async Task<Response<SyncResult<TModel>>> HandleAsync(TSyncCommand command)
        {
            _analyticsService.TraceVerbose(this, $"{typeof(TSyncCommand).Name} handler for {typeof(TModel).Name} started", command.ToObjectDictionary("Command"));

            var commandDto = _converter.Convert(command);
            var dtoResult = await _dtoCommandHandler.HandleAsync(commandDto);
            if (dtoResult.IsFailure)
            {
                return Response.Failure<SyncResult<TModel>>(dtoResult.Error);
            }

            var models = await _repository.SaveSyncedDtosAsync(dtoResult.Value.Dtos);

            _analyticsService.TraceVerbose(this,  $"{typeof(TSyncCommand).Name} handler for {typeof(TModel).Name} succeeded");

            return Response.Success(new SyncResult<TModel>
            {
                EntityBatch = (List<TModel>) models,
                TotalActiveEntityCount = dtoResult.Value.TotalEntityCount,
                EntitiesToDownloadCount = dtoResult.Value.ModifiedEntityCount
            });
        }
    }
}