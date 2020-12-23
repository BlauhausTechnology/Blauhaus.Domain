using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Sync.Abstractions;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncClients
{
    public class SyncClientMockBuilder<TModel, TSyncCommand> : BaseSyncClientMockBuilder<SyncClientMockBuilder<TModel, TSyncCommand>, ISyncClient<TModel, TSyncCommand>, TModel, TSyncCommand> 
        where TModel : IClientEntity 
        where TSyncCommand : SyncCommand
    {
        
    }

    public abstract class BaseSyncClientMockBuilder<TBuilder, TMock, TModel, TSyncCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ISyncClient<TModel, TSyncCommand> 
        where TModel : IClientEntity 
        where TSyncCommand : SyncCommand
        where TBuilder : BaseSyncClientMockBuilder<TBuilder, TMock, TModel, TSyncCommand>
    {


        public TBuilder Where_Connect_returns(IEnumerable<TModel> models)
        {
            Mock.Setup(x => x.Connect(It.IsAny<TSyncCommand>(), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    foreach (var model in models)
                    {
                        observer.OnNext(model);
                    }
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }

        public TBuilder Where_Connect_returns(IDisposable disposable)
        {
            Mock.Setup(x => x.Connect(It.IsAny<TSyncCommand>(), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()))
                .Returns(Observable.Create<TModel>(observer => disposable));

            return (TBuilder) this;
        }

        public TBuilder Where_Connect_returns(TModel model)
        {
            Mock.Setup(x => x.Connect(It.IsAny<TSyncCommand>(), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnNext(model);
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }
        
        public TBuilder Where_Connect_returns_exception(Exception exception)
        {
            Mock.Setup(x => x.Connect(It.IsAny<TSyncCommand>(), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnError(exception);
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }
    }
}