using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Abstractions.Extensions
{
    public static class EntityExtensions
    {
        public static bool IsActive(this IEntity entity)
        {
            return entity.EntityState == EntityState.Active;
        }
        public static bool IsArchived(this IEntity entity)
        {
            return entity.EntityState == EntityState.Archived;
        }
        public static bool IsDeleted(this IEntity entity)
        {
            return entity.EntityState == EntityState.Deleted;
        }
        public static bool IsDraft(this IEntity entity)
        {
            return entity.EntityState == EntityState.Draft;
        }
    }
}