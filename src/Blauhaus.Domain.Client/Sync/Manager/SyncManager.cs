using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientActors.Actors;
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

        public Task<Dictionary<string, long>> GetLastModifiedTicksAsync()
        {
            return InvokeAsync(async () =>
            {
                var dtoSyncClientTasks = new List<Task<KeyValuePair<string, long>>>();
                foreach (var dtoSyncClient in _dtoSyncClients)
                {
                    dtoSyncClientTasks.Add(dtoSyncClient.LoadLastModifiedTicksAsync());
                }

                var dtoLastModifieds = await Task.WhenAll(dtoSyncClientTasks);

                var lastModifiedTicks = new Dictionary<string, long>();
                foreach (var dtoLastModified in dtoLastModifieds)
                {
                    lastModifiedTicks[dtoLastModified.Key] = dtoLastModified.Value;
                }

                return lastModifiedTicks;
            });
        }

        public async Task<Response> SyncAllAsync(Dictionary<string, long>? lastModifiedTicks)
        {
            return await InvokeAsync(async () =>
            {
                _overallStatus = new OverallSyncStatus();

                var dtoSyncClientTasks = new List<Task<Response>>();
                foreach (var dtoSyncClient in _dtoSyncClients)
                {
                    dtoSyncClientTasks.Add(SyncDtoAsync(dtoSyncClient, lastModifiedTicks));
                }

                var syncResults = await Task.WhenAll(dtoSyncClientTasks);
                foreach (var syncResult in syncResults)
                {
                    if (syncResult.IsFailure) return Response.Failure(syncResult.Error);
                }

                return Response.Success();
            });
        }

        private async Task<Response> SyncDtoAsync(IDtoSyncClient dtoSyncClient, Dictionary<string, long>? lastModifiedTicks = null)
        {

            var token = dtoSyncClient.SubscribeAsync(async dtoSyncStatus =>
            {
                _overallStatus = _overallStatus.Update(dtoSyncStatus);
                await UpdateSubscribersAsync(_overallStatus);
            });

            var dtoSyncResult = await dtoSyncClient.SyncDtoAsync(lastModifiedTicks);

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