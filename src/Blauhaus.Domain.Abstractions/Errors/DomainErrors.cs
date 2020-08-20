using System;
using System.Linq.Expressions;
using Blauhaus.Common.Utils.Extensions;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Errors;

namespace Blauhaus.Domain.Abstractions.Errors
{
    public class DomainErrors
    {

        #region NotFound

        public static Error NotFound() => Error.Create("Entity not found");
        public static Error NotFound(string name) => Error.Create($"{name} not found");
        public static Error NotFound<T>() => Error.Create($"{typeof(T).Name} not found");

        #endregion

        #region Duplicate
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

        #endregion

        #region InvalidEntityState
        
        public static Error InvalidEntityState() => Error.Create("Entity state is invalid for this operation");
        public static Error InvalidEntityState(EntityState state) => Error.Create($"{state} is an invalid state for this operation");

        #endregion

        #region InvalidDateFormat
        public static Error InvalidDateFormat = Error.Create("Dates and times must be in UTC format");
        #endregion

        #region InvalidCommand

        public static Error InvalidCommand(string propertyName) => Error.Create($"The value provided for {propertyName} on the command was invalid");
        public static Error InvalidCommand<TCommand>() => Error.Create($"The {typeof(TCommand).Name} was invalid");
        public static Error InvalidCommand<TCommand>(Expression<Func<TCommand, object>> property) => Error.Create($"The value provided for {property.ToPropertyName()} on {typeof(TCommand).Name} was invalid");
        public static Error InvalidCommand<TCommand>(Expression<Func<TCommand, object>> property,  string reason) 
            => Error.Create($"The value provided for {property.ToPropertyName()} on {typeof(TCommand).Name} was invalid: {reason}");

        #endregion
    }
}