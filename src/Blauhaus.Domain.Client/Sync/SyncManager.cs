using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientActors.Actors;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.SyncClient;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Client.Sync.Manager
{
    public class SyncManager : BaseActor, ISyncManager
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IEnumerable<IDtoSyncClient> _dtoSyncClients;
        private OverallSyncStatus _overallStatus = null!;

        public SyncManager(
            IAnalyticsService analyticsService,
            IEnumerable<IDtoSyncClient> dtoSyncClients)
        {
            _analyticsService = analyticsService;
            _dtoSyncClients = dtoSyncClients;
        }

        public async Task<Response> SyncAllAsync(IKeyValueProvider? settingsProvider)
        {
            return await InvokeAsync(async () =>
            {
                using var _ = _analyticsService.StartTrace(this, "Sync");

                _overallStatus = new OverallSyncStatus();

                var dtoSyncClientTasks = new List<Task<Response>>();
                foreach (var dtoSyncClient in _dtoSyncClients)
                {
                    dtoSyncClientTasks.Add(SyncDtoAsync(dtoSyncClient, settingsProvider));
                }

                var syncResults = await Task.WhenAll(dtoSyncClientTasks);
                foreach (var syncResult in syncResults)
                {
                    if (syncResult.IsFailure) return Response.Failure(syncResult.Error);
                }

                return Response.Success();
            });
        }
         
        private async Task<Response> SyncDtoAsync(IDtoSyncClient dtoSyncClient, IKeyValueProvider? settingsProvider)
        {
            var token = dtoSyncClient.SubscribeAsync(async dtoSyncStatus =>
            {
                _overallStatus = _overallStatus.Update(dtoSyncStatus);
                await UpdateSubscribersAsync(_overallStatus);
            });

            var dtoSyncResult = await dtoSyncClient.SyncDtoAsync(settingsProvider);

            token?.Dispose();

            return dtoSyncResult.IsFailure 
                ? Response.Failure(dtoSyncResult.Error) 
                : Response.Success();
        }

        public Task<IDisposable> SubscribeAsync(Func<IOverallSyncStatus, Task> handler, Func<IOverallSyncStatus, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

    }
}