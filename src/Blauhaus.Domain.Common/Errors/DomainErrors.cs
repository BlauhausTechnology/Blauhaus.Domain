using System;
using System.Linq.Expressions;
using Blauhaus.Common.Utils.Extensions;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Common.Errors
{
    public class DomainErrors
    {
        public static Error NotFound() => Error.Create("Entity not found");
        public static Error NotFound(string name) => Error.Create($"{name} not found");
        public static Error NotFound<T>() => Error.Create($"{typeof(T).Name} not found");

        
        public static Error Duplicate() 
            => Error.Create("This entity already exists");

        public static Error Duplicate(string entityName, string propertyName) 
            => Error.Create($"{entityName} already exists with this value for {propertyName}");

        public static Error Duplicate(string entityName, string propertyName, object value) 
            => Error.Create($"{entityName} already exists with {propertyName} == {value}");
        
        public static Error Duplicate<T>(Expression<Func<T, object>> expression) 
            => Error.Create($"{typeof(T).Name} already exists with this value for {expression.ToPropertyName()}");
        
        public static Error Duplicate<T>(Expression<Func<T, object>> expression, object value) 
            => Error.Create($"{typeof(T).Name} already exists with {expression.ToPropertyName()} == {value}");


        public static Error InvalidDateFormat = Error.Create("Dates and times must be in UTC format");
         
    }
}