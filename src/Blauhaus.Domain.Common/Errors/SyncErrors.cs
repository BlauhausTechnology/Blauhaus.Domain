﻿using Blauhaus.Common.ValueObjects.Errors;

namespace Blauhaus.Domain.Common.Errors
{
    public static class SyncErrors
    {
        public static readonly Error InvalidSyncCommand = Error.Create("SyncCommand must provide NewerThan to load new entities or OlderThan to load older entities, but cannot specify both");
    }
}