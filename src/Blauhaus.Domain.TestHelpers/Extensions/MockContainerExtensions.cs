using System;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientRepositoryHelpers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.TestHelpers;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockContainerExtensions
    {

        
        public static Func<ClientEntityConverterMockBuilder<TModel, TDto, TRootEntity>> AddMockClientEntityConverter<TModel, TDto, TRootEntity>(this MockContainer mocks) where TModel : class, IClientEntity where TRootEntity : ISyncClientEntity, new() 
            => mocks.AddMock<ClientEntityConverterMockBuilder<TModel, TDto, TRootEntity>, IClientEntityConverter<TModel, TDto, TRootEntity>>();

        public static Func<ClientRepositoryMockBuilder<TModel, TDto>> AddMockClientRepository<TModel, TDto>(this MockContainer mocks) where TModel : class, IClientEntity
            => mocks.AddMock<ClientRepositoryMockBuilder<TModel, TDto>, IClientRepository<TModel, TDto>>();

        public static Func<BaseSyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>> AddMockSyncClientRepository<TModel, TDto, TSyncCommand>(this MockContainer mocks) where TModel : class, IClientEntity where TSyncCommand : SyncCommand
            => mocks.AddMock<BaseSyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>, ISyncClientRepository<TModel, TDto, TSyncCommand>>();
        
        public static Func<SyncClientMockBuilder<TModel, SyncCommand>> AddMockSyncClient<TModel>(this MockContainer mocks) where TModel : class, IClientEntity 
          => mocks.AddMockSyncClient<TModel, SyncCommand>();
        
        public static Func<SyncClientMockBuilder<TModel, TSyncCommand>> AddMockSyncClient<TModel, TSyncCommand>(this MockContainer mocks) where TModel : class, IClientEntity where TSyncCommand : SyncCommand
             => mocks.AddMock<SyncClientMockBuilder<TModel, TSyncCommand>, ISyncClient<TModel, TSyncCommand>>();

        public static Func<CommandHandlerMockBuilder<TModel, TCommand>> AddMockCommandHandler<TModel, TCommand>(this MockContainer mocks)  where TCommand : notnull
            => mocks.AddMock<CommandHandlerMockBuilder<TModel, TCommand>, ICommandHandler<TModel, TCommand>>();
        
        public static Func<VoidCommandHandlerMockBuilder<TCommand>> AddMockVoidCommandHandler<TCommand>(this MockContainer mocks)   where TCommand : notnull  
            => mocks.AddMock<VoidCommandHandlerMockBuilder<TCommand>, IVoidCommandHandler<TCommand>>();
    }
}