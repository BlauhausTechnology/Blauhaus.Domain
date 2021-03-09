using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.Extensions
{
    public static class ClientEntityExtensions
    {
        public static bool IsOlderThan(this IClientEntity entity, long modifiedAtTicks)
        {

            return modifiedAtTicks == 0 || entity.ModifiedAtTicks < modifiedAtTicks;
        }
        
        public static bool IsNewerThan(this IClientEntity entity, long modifiedAtTicks)
        {
            return modifiedAtTicks == 0 || entity.ModifiedAtTicks > modifiedAtTicks;
        }
    }
}