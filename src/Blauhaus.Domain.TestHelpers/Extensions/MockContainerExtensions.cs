using System;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientEntityConverters;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncCollections;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncModels;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.TestHelpers;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Sync.Old.Collection;
using Blauhaus.Domain.Client.Sync.Old.Model;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoHandlers;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockContainerExtensions
    {

        //DtoCaches
        public static Func<SyncDtoCacheMockBuilder<TDto, TId>> AddMockSyncDtoCache<TDto, TId>(this MockContainer mocks) where TDto : class, IClientEntity<TId> where TId : IEquatable<TId> => mocks.AddMock<SyncDtoCacheMockBuilder<TDto, TId>, ISyncDtoCache<TDto,TId>>();
        public static Func<DtoCacheMockBuilder<TDto, TId>> AddMockDtoCache<TDto, TId>(this MockContainer mocks) where TDto : class, IHasId<TId> where TId : IEquatable<TId> => mocks.AddMock<DtoCacheMockBuilder<TDto, TId>, IDtoCache<TDto, TId>>();
        public static Func<DtoHandlerMockBuilder<TDto, TId>> AddMockDtoHandler<TDto, TId>(this MockContainer mocks) where TDto : class, IHasId<TId>
            => mocks.AddMock<DtoHandlerMockBuilder<TDto, TId>, IDtoHandler<TDto>>();

        //Client entity converters
        public static Func<ClientEntityConverterMockBuilder<TModel, TDto, TRootEntity>> AddMockClientEntityConverter<TModel, TDto, TRootEntity>(this MockContainer mocks) where TModel : class, IClientEntity<Guid> where TRootEntity : ISyncClientEntity<Guid>, new() 
            => mocks.AddMock<ClientEntityConverterMockBuilder<TModel, TDto, TRootEntity>, IClientEntityConverter<TModel, TDto, TRootEntity>>();


        //Repositories
        public static Func<ClientRepositoryMockBuilder<TModel, TDto>> AddMockClientRepository<TModel, TDto>(this MockContainer mocks) where TModel : class, IClientEntity<Guid>
            => mocks.AddMock<ClientRepositoryMockBuilder<TModel, TDto>, IClientRepository<TModel, TDto>>();

        public static Func<SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>> AddMockSyncClientRepository<TModel, TDto, TSyncCommand>(this MockContainer mocks) where TModel : class, IClientEntity<Guid> where TSyncCommand : SyncCommand
            => mocks.AddMock<SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>, ISyncClientRepository<TModel, TDto, TSyncCommand>>();
        

        //Sync clients
        public static Func<SyncClientMockBuilder<TModel, SyncCommand>> AddMockSyncClient<TModel>(this MockContainer mocks) where TModel : class, IClientEntity <Guid>
          => mocks.AddMockSyncClient<TModel, SyncCommand>();
        
        public static Func<SyncClientMockBuilder<TModel, TSyncCommand>> AddMockSyncClient<TModel, TSyncCommand>(this MockContainer mocks) where TModel : class, IClientEntity<Guid> where TSyncCommand : SyncCommand
             => mocks.AddMock<SyncClientMockBuilder<TModel, TSyncCommand>, ISyncClient<TModel, TSyncCommand>>();


        //handlers 
        
        public static Func<HandlerMockBuilder<TModel>> AddMockHandler<TModel>(this MockContainer mocks)  
            => mocks.AddMock<HandlerMockBuilder<TModel>, IHandler<TModel>>();

        public static Func<CommandHandlerMockBuilder<TModel, TCommand>> AddMockCommandHandler<TModel, TCommand>(this MockContainer mocks)  where TCommand : notnull
            => mocks.AddMock<CommandHandlerMockBuilder<TModel, TCommand>, ICommandHandler<TModel, TCommand>>();
        
        public static Func<VoidCommandHandlerMockBuilder<TCommand>> AddMockVoidCommandHandler<TCommand>(this MockContainer mocks)   where TCommand : notnull  
            => mocks.AddMock<VoidCommandHandlerMockBuilder<TCommand>, IVoidCommandHandler<TCommand>>();

        public static Func<AuthenticatedCommandHandlerMockBuilder<TModel, TCommand, TUser>> AddMockAuthenticatedCommandHandler<TModel, TCommand, TUser>(this MockContainer mocks)  where TCommand : notnull
            => mocks.AddMock<AuthenticatedCommandHandlerMockBuilder<TModel, TCommand, TUser>, IAuthenticatedCommandHandler<TModel, TCommand, TUser>>();

        public static Func<VoidAuthenticatedCommandHandlerMockBuilder<TCommand, TUser>> AddMockVoidAuthenticatedCommandHandler<TCommand, TUser>(this MockContainer mocks)  where TCommand : notnull
            => mocks.AddMock<VoidAuthenticatedCommandHandlerMockBuilder<TCommand, TUser>, IVoidAuthenticatedCommandHandler<TCommand, TUser>>();


        //Sync collections
        public static Func<SyncCollectionMockBuilder<TModel, TListItem>> AddMockSyncCollection<TModel, TListItem>(this MockContainer mocks) where TModel : IClientEntity<Guid> where TListItem : IListItem<TModel> 
            => mocks.AddMock<SyncCollectionMockBuilder<TModel, TListItem>, ISyncCollection<TModel, TListItem, SyncCommand>>();

        public static Func<SyncCollectionMockBuilder<TModel, TListItem, TCommand>> AddMockSyncCollection<TModel, TListItem, TCommand>(this MockContainer mocks) where TCommand : SyncCommand, new() where TModel : IClientEntity<Guid> where TListItem : IListItem<TModel> 
            => mocks.AddMock<SyncCollectionMockBuilder<TModel, TListItem, TCommand>, ISyncCollection<TModel, TListItem, TCommand>>();


        //Sync models
        public static Func<SyncModelMockBuilder<TModel>> AddMockSyncModel<TModel>(this MockContainer mocks) where TModel : IClientEntity <Guid>
            => mocks.AddMock<SyncModelMockBuilder<TModel>, ISyncModel<TModel>>();

    }
}