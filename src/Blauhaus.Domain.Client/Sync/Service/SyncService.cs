using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;

namespace Blauhaus.Domain.Client.Sync.Service
{
    public class SyncService<TSyncCommand> : ISyncService 
        where TSyncCommand : SyncCommand, new()
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISyncClientFactory<TSyncCommand> _syncClientFactory;
        private readonly ISyncStatusHandlerFactory _syncStatusHandlerFactory;

        public SyncService(
            IAnalyticsService analyticsService,
            ISyncClientFactory<TSyncCommand> syncClientFactory,
            ISyncStatusHandlerFactory syncStatusHandlerFactory)
        {
            _analyticsService = analyticsService;
            _syncClientFactory = syncClientFactory;
            _syncStatusHandlerFactory = syncStatusHandlerFactory;
        }

        public IObservable<SyncUpdate> Sync() 
        {
            return Observable.Create<SyncUpdate>(observer =>
            {
                var disposables = new CompositeDisposable();
                var syncClients = _syncClientFactory.SyncConnections;
                var syncUpdate = new SyncUpdateState(syncClients.Count);

                observer.OnNext(syncUpdate.Publish());
                _analyticsService.TraceInformation(this, $"Sync started for {syncUpdate.EntityTypesToSync} entity types");

                for (var i = 0; i < syncClients.Count; i++)
                {
                    var id = i;
                    var syncClient = syncClients[i];
                    var syncStatusHandler = _syncStatusHandlerFactory.Get();

                    void HandleStatusChanged(object s, PropertyChangedEventArgs e)
                    {
                        var nextUpdate = syncUpdate.Update(id, syncStatusHandler);
                        observer.OnNext(nextUpdate);

                        if (nextUpdate.IsCompleted)
                        {
                            _analyticsService.TraceInformation(this, $"Sync completed for {syncUpdate.EntityTypesToSync} entity types. {nextUpdate.EntitiesSynced} entities synced");
                            observer.OnCompleted();
                        }
                    };

                    disposables.Add(Observable.FromEventPattern(
                        x => syncStatusHandler.PropertyChanged += HandleStatusChanged, 
                        x => syncStatusHandler.PropertyChanged -= HandleStatusChanged)
                            .Subscribe());

                    disposables.Add(syncClient.Invoke(new TSyncCommand(), ClientSyncRequirement.All, syncStatusHandler)
                        .Subscribe()); 
                }

                return disposables;
               
            });

        }

    }
}