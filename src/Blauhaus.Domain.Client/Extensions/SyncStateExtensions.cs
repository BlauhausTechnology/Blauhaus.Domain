﻿using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.Sync.Client;

namespace Blauhaus.Domain.Client.Extensions
{
    public static class SyncStateExtensions
    {
        public static bool IsExecuting(this SyncClientState state)
        {
            return state == SyncClientState.DownloadingNew
                   || state == SyncClientState.LoadingLocal
                   || state == SyncClientState.DownloadingOld;
        }
    }
}