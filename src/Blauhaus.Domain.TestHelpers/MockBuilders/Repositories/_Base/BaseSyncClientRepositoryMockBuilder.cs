using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Repositories._Base
{
    public class SyncClientRepositoryMockBuilder<TMock, TModel, TDto> : BaseSyncClientRepositoryMockBuilder<SyncClientRepositoryMockBuilder<TMock, TModel, TDto>, TMock, TModel, TDto>
        where TMock : class, ISyncClientRepository<TModel, TDto> 
        where TModel : class, IClientEntity
    {
    }
     

    public abstract class BaseSyncClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto> : BaseClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto> 
        where TBuilder : BaseSyncClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto>
        where TMock : class, ISyncClientRepository<TModel, TDto>
        where TModel : class, IClientEntity
    {
        
        public TBuilder Where_LoadSyncedModelsAsync_returns(TModel model)
        {
            Mock.Setup(x => x.LoadSyncedModelsAsync(It.IsAny<long?>(), It.IsAny<int>()))
                .ReturnsAsync(new List<TModel>{model});
            return this as TBuilder;
        }
        public TBuilder Where_LoadSyncedModelsAsync_returns(List<TModel> models)
        {
            Mock.Setup(x => x.LoadSyncedModelsAsync(It.IsAny<long?>(), It.IsAny<int>()))
                .ReturnsAsync(models);
            return this as TBuilder;
        }
        public TBuilder Where_LoadSyncedModelsAsync_throws(Exception e)
        {
            Mock.Setup(x => x.LoadSyncedModelsAsync(It.IsAny<long?>(), It.IsAny<int>()))
                .ThrowsAsync(e);
            return this as TBuilder;
        }

        
        public TBuilder Where_GetSyncStatusAsync_throws(Exception e)
        {
            Mock.Setup(x => x.GetSyncStatusAsync())
                .ThrowsAsync(e);
            return this as TBuilder;
        }

        
        public TBuilder Where_SaveSyncedDtosAsync_returns(IReadOnlyList<TModel> models)
        {
            Mock.Setup(x => x.SaveSyncedDtosAsync(It.IsAny<IReadOnlyList<TDto>>()))
                .ReturnsAsync(models);
            return this as TBuilder;
        }

        
        public TBuilder Where_GetSyncStatusAsync_returns(ClientSyncStatus value)
        {
            Mock.Setup(x => x.GetSyncStatusAsync())
                .ReturnsAsync(value);
            return this as TBuilder;
        }

    
    }

}