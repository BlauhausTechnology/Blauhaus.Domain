using System.Linq;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Microsoft.EntityFrameworkCore;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
    public static class NamedEntityDbSetExtensions
    {
        public static Response<string> ValidateNamedEntityCreateCommand<TEntity>(this DbSet<TEntity> dbSet, IHasName command, int minimumLength = 4) 
            where TEntity : class, IHasName, IServerEntity
        {

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Response.Failure<string>(Error.RequiredValue<IHasName>(x => x.Name));
            }

            var requestedName = command.Name.TrimStart().TrimEnd();

            if (requestedName.Length < minimumLength)
            {
                return Response.Failure<string>(Error.InvalidValue<IHasName>(x => x.Name, $"Name must be at least {minimumLength} characters"));
            } 

            var matchedByName = dbSet.AsNoTracking().FirstOrDefault(x => 
                EF.Functions.Like(x.Name, command.Name.TrimStart().TrimEnd()) &&
                x.EntityState != EntityState.Deleted);
            
            return matchedByName != null 
                ? Response.Failure<string>(DomainErrors.Duplicate<IHasName>(x => x.Name, command.Name)) 
                : Response.Success(requestedName);
        }
    }
}