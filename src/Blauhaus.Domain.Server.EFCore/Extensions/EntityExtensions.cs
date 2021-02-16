using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
    public static class EntityExtensions
    {
        public static TEntity ThrowIfNotExists<TEntity>(this TEntity? entity)
        {
            if (entity == null)
            {
                throw new ErrorException(DomainErrors.NotFound<TEntity>());
            }

            return entity;
        }
    }
}