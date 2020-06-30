using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
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
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
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

            return this as TBuilder;
        }

        public TBuilder Where_Connect_returns(TModel model)
        {
            Mock.Setup(x => x.Connect(It.IsAny<TSyncCommand>(), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnNext(model);
                    return Disposable.Empty;
                }));

            return this as TBuilder;
        }
        
        public TBuilder Where_Connect_returns_exception(Exception exception)
        {
            Mock.Setup(x => x.Connect(It.IsAny<TSyncCommand>(), It.IsAny<ClientSyncRequirement>(), It.IsAny<ISyncStatusHandler>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnError(exception);
                    return Disposable.Empty;
                }));

            return this as TBuilder;
        }
    }
}