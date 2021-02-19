using System.Linq;
using Blauhaus.Common.Utils.Contracts;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Responses;
using Microsoft.EntityFrameworkCore;

namespace Blauhaus.Domain.Server.EFCore.Extensions
{
    public static class NamedEntityDbSetExtensions
    {
        public static Response<string> ValidateNamedEntityCreateCommand<TEntity>(this DbSet<TEntity> dbSet, IHasName command, int minimumLength = 4) 
            where TEntity : class, IHasName
        {

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Response.Failure<string>(Errors.Errors.InvalidValue<IHasName>(x => x.Name, "Name is required"));
            }

            var requestedName = command.Name.TrimStart().TrimEnd();

            if (requestedName.Length < minimumLength)
            {
                return Response.Failure<string>(Errors.Errors.InvalidValue<IHasName>(x => x.Name, $"Name must be at least {minimumLength} characters"));
            } 

            var matchedByName = dbSet.AsNoTracking().FirstOrDefault(x => EF.Functions.Like(x.Name, command.Name.TrimStart().TrimEnd()));
            if (matchedByName != null)
            {
                return Response.Failure<string>(DomainErrors.Duplicate<IHasName>(x => x.Name, command.Name));
            }

            return Response.Success(requestedName);
        }
    }
}