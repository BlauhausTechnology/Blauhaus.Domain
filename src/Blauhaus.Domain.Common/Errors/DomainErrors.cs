using Blauhaus.Errors;

namespace Blauhaus.Domain.Common.Errors
{
    public class DomainErrors
    {
        public static Error NotFound(string name = "") => string.IsNullOrEmpty(name) 
            ? Error.Create("Not found") 
            : Error.Create($"{name} Not found");

        public static Error InvalidDateFormat = Error.Create("Dates and times must be in UTC format");
         
    }
}