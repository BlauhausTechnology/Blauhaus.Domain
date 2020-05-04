using System;

namespace Blauhaus.Domain.Common.Extensions
{
    public static class LongExtensions
    {
        public static DateTime ToUtcDateTime(this long ticks)
        {
            return DateTime.SpecifyKind(new DateTime(ticks), DateTimeKind.Utc);
        }

        public static DateTime? ToUtcDateTime(this long? ticks)
        {
            return ticks == null 
                ? (DateTime?) null 
                : DateTime.SpecifyKind(new DateTime(ticks.Value), DateTimeKind.Utc);
        }
    }
}