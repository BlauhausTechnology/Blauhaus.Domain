using System;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers;
using Blauhaus.TestHelpers;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches;
using Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoHandlers;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common.DtoCaches;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockContainerExtensions
    {

        //DtoCaches
        public static Func<DtoCacheMockBuilder<TDto, TId>> AddMockDtoCache<TDto, TId>(this MockContainer mocks) where TDto : class, IHasId<TId> where TId : IEquatable<TId> => mocks.AddMock<DtoCacheMockBuilder<TDto, TId>, IDtoCache<TDto, TId>>();
        public static Func<DtoHandlerMockBuilder<TDto, TId>> AddMockDtoHandler<TDto, TId>(this MockContainer mocks) where TDto : class, IHasId<TId>
            => mocks.AddMock<DtoHandlerMockBuilder<TDto, TId>, IDtoHandler<TDto>>();

        //DtoLoaders
        public static Func<DtoLoaderMockBuilder<TDto, TId>> AddMockDtoLoader<TDto, TId>(this MockContainer mocks) where TDto : class, IHasId<TId> where TId : IEquatable<TId> 
            => mocks.AddMock<DtoLoaderMockBuilder<TDto, TId>, IDtoLoader<TDto, TId>>();


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

    }
}