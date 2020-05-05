using System;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.TestHelpers.MockBuilders.Repositories;
using Blauhaus.TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockContainerExtensions
    {
        public static Func<ClientRepositoryMockBuilder<TModel, TDto>> AddMockClientRepository<TModel, TDto>(this MockContainer mocks) where TModel : class, IClientEntity
        {
            return mocks.AddMock<ClientRepositoryMockBuilder<TModel, TDto>, IClientRepository<TModel, TDto>>();
        }

        public static Func<SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>> AddMockSyncClientRepository<TModel, TDto, TSyncCommand>(this MockContainer mocks) where TModel : class, IClientEntity where TSyncCommand : SyncCommand
        {
            return mocks.AddMock<SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>, ISyncClientRepository<TModel, TDto, TSyncCommand>>();
        }
    }
}