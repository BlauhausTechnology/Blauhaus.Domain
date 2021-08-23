using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Moq;
using Newtonsoft.Json;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories
{
    public class SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand> 
        : BaseSyncClientRepositoryMockBuilder<SyncClientRepositoryMockBuilder<TModel, TDto, TSyncCommand>,  ISyncClientRepository<TModel, TDto, TSyncCommand> , TModel, TDto, TSyncCommand>
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
    {
    }
     

    public abstract class BaseSyncClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto, TSyncCommand> : BaseClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto> 
        where TBuilder : BaseSyncClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto, TSyncCommand>
        where TMock : class, ISyncClientRepository<TModel, TDto, TSyncCommand>
        where TModel : class, IClientEntity
        where TSyncCommand : SyncCommand
    {
        
        public TBuilder Where_LoadModelsAsync_returns(TModel model)
        {
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
                .ReturnsAsync(new List<TModel>{model});
            return (TBuilder) this;
        }

        public TBuilder Where_LoadModelsAsync_returns(List<TModel> models)
        {
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
                .ReturnsAsync(models);
            return (TBuilder) this;
        }
        
        public List<TSyncCommand> Where_LoadModelsAsync_returns_sequence(List<List<TModel>> models)
        {
            var invokedWithCommands = new List<TSyncCommand>();
            var queue = new Queue<List<TModel>>(models);
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
                .Callback((TSyncCommand syncCommand) =>
                {
                    invokedWithCommands.Add(JsonConvert.DeserializeObject<TSyncCommand>(JsonConvert.SerializeObject(syncCommand)));
                })
                .ReturnsAsync(queue.Dequeue);
            return invokedWithCommands;
        }

        public TBuilder Where_LoadModelsAsync_throws(Exception e)
        {
            Mock.Setup(x => x.LoadModelsAsync(It.IsAny<TSyncCommand>()))
                .ThrowsAsync(e);
            return (TBuilder) this;
        }

        
        public TBuilder Where_GetSyncStatusAsync_throws(Exception e)
        {
            Mock.Setup(x => x.GetSyncStatusAsync(It.IsAny<TSyncCommand>()))
                .ThrowsAsync(e);
            return (TBuilder) this;
        }

        
        public TBuilder Where_SaveSyncedDtosAsync_returns(IReadOnlyList<TModel> models)
        {
            Mock.Setup(x => x.SaveSyncedDtosAsync(It.IsAny<IReadOnlyList<TDto>>()))
                .ReturnsAsync(models);
            return (TBuilder) this;
        }
        
        public TBuilder Where_GetSyncStatusAsync_returns(ClientSyncStatus value)
        {
            Mock.Setup(x => x.GetSyncStatusAsync(It.IsAny<TSyncCommand>()))
                .ReturnsAsync(value);
            return (TBuilder) this;
        }

        public TBuilder Where_GetSyncStatusAsync_returns_sequence(List<ClientSyncStatus> values)
        {
            var queue = new Queue<ClientSyncStatus>(values.ToList());
            Mock.Setup(x => x.GetSyncStatusAsync(It.IsAny<TSyncCommand>()))
                .ReturnsAsync(queue.Dequeue);
            return (TBuilder) this;
        }
         
    
    }

}