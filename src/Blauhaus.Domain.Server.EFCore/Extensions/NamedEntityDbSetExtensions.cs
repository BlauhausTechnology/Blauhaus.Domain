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
        public static Response<string> ValidateName<TEntity>(this DbSet<TEntity> dbSet, string name, int? min = null, int? max = null) 
            where TEntity : class, IHasName
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Response.Failure<string>(Error.InvalidValue<IHasName>(x => x.Name, "Name is required"));
            }

            name = name.TrimStart().TrimEnd();

            if (min != null && name.Length < min)
            {
                return Response.Failure<string>(Error.InvalidValue<IHasName>(x => x.Name, $"Name must be at least {min} characters"));
            }
             
            if (max != null && name.Length > max)
            {
                return Response.Failure<string>(Error.InvalidValue<IHasName>(x => x.Name, $"Name can be no more than {max} characters"));
            }

            var matchedByName = dbSet.AsNoTracking().FirstOrDefault(x => EF.Functions.Like(x.Name, name));
            if (matchedByName != null)
            {
                return Response.Failure<string>(DomainErrors.Duplicate<IHasName>(x => x.Name, name));
            }

            return Response.Success(name);
        }

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