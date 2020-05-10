using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Common.Extensions
{
    public static class EntityExtensions
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