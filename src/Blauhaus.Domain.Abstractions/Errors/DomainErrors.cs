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

        [Obsolete("Moved to Blauhaus.Error")]
        public static Error NotFound() => Error.Create("The requested object was not found");
        [Obsolete("Moved to Blauhaus.Error")]
        public static Error NotFound(string name) => Error.Create($"{name} was not found");
        [Obsolete("Moved to Blauhaus.Error")]
        public static Error NotFound<T>() => Error.Create($"{typeof(T).Name} was not found");

        #endregion

        #region Duplicate
        public static Error Duplicate() 
            => Error.Create("This entity already exists");

        public static Error Duplicate(string objectName, string propertyName) 
            => Error.Create($"{objectName} already exists with this value for {propertyName}");

        public static Error Duplicate(string objectName, string propertyName, object value) 
            => Error.Create($"{objectName} already exists with {propertyName} == {value}");
        
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


         
    }
}