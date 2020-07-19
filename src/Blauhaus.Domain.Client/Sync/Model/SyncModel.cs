using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Results;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Common.Errors;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Client.Sync.Model
{
    public class SyncModel<TModel, TSyncCommand> : ISyncModel<TModel> 
        where TModel : IClientEntity 
        where TSyncCommand : SyncCommand, new()
    {
        private readonly ISyncClient<TModel, TSyncCommand> _syncClient;
        private readonly ISyncStatusHandler _syncStatusHandler;
        private readonly IAnalyticsService _analyticsService;

        public SyncModel(
            ISyncClient<TModel, TSyncCommand> syncClient,
            ISyncStatusHandler syncStatusHandler,
            IAnalyticsService analyticsService)
        {
            _syncClient = syncClient;
            _syncStatusHandler = syncStatusHandler;
            _analyticsService = analyticsService;
        }

        public IObservable<TModel> Connect(Guid id)
        {
           
            return Observable.Create<TModel>(observer =>
            {
                void OnSyncStatusChanged(object sender, PropertyChangedEventArgs e)
                {
                    var statusHandler = (ISyncStatusHandler) sender;
                    if (statusHandler.AllServerEntities != null && statusHandler.AllServerEntities == 0)
                    {
                        var error = DomainErrors.NotFound<TModel>();
                        _analyticsService.TraceWarning(this, error.ToString());
                        observer.OnError(new ErrorException(DomainErrors.NotFound<TModel>()));
                    }
                }
                
                var syncStatusConnect = Observable.FromEventPattern(
                    x => _syncStatusHandler.PropertyChanged += OnSyncStatusChanged,
                    x => _syncStatusHandler.PropertyChanged -= OnSyncStatusChanged).Subscribe();

                var syncCommand = new TSyncCommand
                {
                    Id = id,
                    BatchSize = 1
                };

                var syncConnection = _syncClient.Connect(syncCommand, ClientSyncRequirement.Minimum(1), _syncStatusHandler)
                    .Subscribe(next =>
                        {
                            if (next.Id == id)
                            {
                                observer.OnNext(next);
                            }
                        },
                        observer.OnError);

                return new CompositeDisposable(syncStatusConnect, syncConnection);
            });
        }
         
        public void LoadNewFromServer()
        {
            _syncClient.LoadNewFromServer();
        }

        public void LoadNewFromClient()
        {
            _syncClient.LoadNewFromClient();
        }
    }
}