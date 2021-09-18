using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sync.Old.Model;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncModels
{

    public abstract class BaseSyncModelMockBuilder<TBuilder, TSyncModel, TModel> : BaseMockBuilder<TBuilder, TSyncModel>
        where TSyncModel : class, ISyncModel<TModel> 
        where TModel : IClientEntity <Guid>
        where TBuilder : BaseSyncModelMockBuilder<TBuilder, TSyncModel, TModel>
    {
        protected BaseSyncModelMockBuilder()
        {
            Where_Connect_returns(new List<TModel>());
        }

        public TBuilder Where_Connect_returns(IEnumerable<TModel> models)
        {
            Mock.Setup(x => x.Connect(It.IsAny<Guid>()))
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
            Mock.Setup(x => x.Connect(It.IsAny<Guid>()))
                .Returns(Observable.Create<TModel>(observer => disposable));

            return (TBuilder) this;
        }

        public TBuilder Where_Connect_returns(TModel model)
        {
            Mock.Setup(x => x.Connect(It.IsAny<Guid>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnNext(model);
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }
        
        public TBuilder Where_Connect_returns_exception(Exception exception)
        {
            Mock.Setup(x => x.Connect(It.IsAny<Guid>()))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnError(exception);
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }

        public TBuilder Where_Connect_returns(IEnumerable<TModel> models, Guid id)
        {
            Mock.Setup(x => x.Connect(id))
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

        public TBuilder Where_Connect_returns(IDisposable disposable, Guid id)
        {
            Mock.Setup(x => x.Connect(id))
                .Returns(Observable.Create<TModel>(observer => disposable));

            return (TBuilder) this;
        }

        public TBuilder Where_Connect_returns(TModel model, Guid id)
        {
            Mock.Setup(x => x.Connect(id))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnNext(model);
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }
        
        public TBuilder Where_Connect_returns_exception(Exception exception, Guid id)
        {
            Mock.Setup(x => x.Connect(id))
                .Returns(Observable.Create<TModel>(observer =>
                {
                    observer.OnError(exception);
                    return Disposable.Empty;
                }));

            return (TBuilder) this;
        }
    }
}