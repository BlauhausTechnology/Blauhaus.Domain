using System;
using System.Collections.Generic;
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

        public Task<Response> SyncAllAsync(Dictionary<string, long>? lastModifiedTicks)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(Func<ISyncStatus, Task> handler, Func<ISyncStatus, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

    }
}