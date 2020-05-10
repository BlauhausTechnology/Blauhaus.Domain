using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories
{
    public class SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand> 
        : SyncClientRepositoryMockBuilder<SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>,  ISyncClientRepository<TModel, TDto, TSyncCommand> , TModel, TDto, TSyncCommand>
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
    {
    }
     

    public abstract class SyncClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto, TSyncCommand> : BaseClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto> 
        where TBuilder : SyncClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto, TSyncCommand>
        where TMock : class, ISyncClientRepository<TModel, TDto, TSyncCommand>
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
    {
        
        public TBuilder Where_LoadSyncedModelsAsync_returns(TModel model)
        {
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
                .ReturnsAsync(new List<TModel>{model});
            return this as TBuilder;
        }
        public TBuilder Where_LoadSyncedModelsAsync_returns(List<TModel> models)
        {
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
                .ReturnsAsync(models);
            return this as TBuilder;
        }
        public TBuilder Where_LoadSyncedModelsAsync_throws(Exception e)
        {
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
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

        public TBuilder Where_GetSyncStatusAsync_returns(List<ClientSyncStatus> values)
        {
            var queue = new Queue<ClientSyncStatus>(values.ToList());
            Mock.Setup(x => x.GetSyncStatusAsync())
                .ReturnsAsync(queue.Dequeue);
            return this as TBuilder;
        }
         
    
    }

}