using System;
using System.Linq.Expressions;
using Blauhaus.Common.Utils.Extensions;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Abstractions.Errors
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

        public static Error InvalidState() => Error.Create("Entity state is invalid for this operation");
        public static Error InvalidState(EntityState state) => Error.Create($"{state} is an invalid state for this operation");


        public static Error InvalidDateFormat = Error.Create("Dates and times must be in UTC format");
         
    }
}