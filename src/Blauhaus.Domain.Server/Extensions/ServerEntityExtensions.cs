using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Server.Extensions
{
    public static class ServerEntityExtensions
    {
        public static Response TryDelete(this BaseServerEntity entity, DateTime now)
        {
            switch (entity.EntityState)
            {
                case EntityState.Deleted:
                    return Response.Success();
                case EntityState.Active:
                    return Response.Failure(DomainErrors.InvalidEntityState(entity.EntityState));
                case EntityState.Draft:
                case EntityState.Archived:
                    entity.Delete(now);
                    return Response.Success();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Response TryArchive(this BaseServerEntity entity, DateTime now)
        {
            switch (entity.EntityState)
            {
                case EntityState.Archived:
                    return Response.Success();
                case EntityState.Draft:
                case EntityState.Deleted:
                    return Response.Failure(DomainErrors.InvalidEntityState(entity.EntityState));
                case EntityState.Active:
                    entity.Archive(now);
                    return Response.Success();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static Response TryActivate(this BaseServerEntity entity, DateTime now)
        {
            switch (entity.EntityState)
            {
                case EntityState.Active:
                    return Response.Success();
                case EntityState.Deleted:
                    return Response.Failure(DomainErrors.InvalidEntityState(entity.EntityState));
                case EntityState.Draft:
                case EntityState.Archived:
                    entity.Activate(now);
                    return Response.Success();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        
        public static bool CanRemove(this BaseServerEntity entity)
        {
            return entity.EntityState == EntityState.Deleted;
        }
    }
}