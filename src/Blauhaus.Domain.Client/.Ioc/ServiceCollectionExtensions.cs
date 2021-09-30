using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;

namespace Blauhaus.Domain.Client.Ioc
{
    public static class ServiceCollectionExtensions
    {
 
        public static IServiceCollection AddDtoHandler<TDto, TId, TDtoHandler>(this IServiceCollection services) 
            where TDto : class, IHasId<TId>
            where TDtoHandler : class, IDtoHandler<TDto>
        {
            services.AddSingleton<Func<TId, Task<IDtoHandler<TDto>>>>(sp => id => Task.FromResult<IDtoHandler<TDto>>(sp.GetRequiredService<TDtoHandler>()));
            return services;
        }
        
        public static IServiceCollection AddDtoHandler<TDto, TId, TDtoHandler>(this IServiceCollection services, Func<IServiceProvider, TId, Task<TDtoHandler>> resolver) 
            where TDto : class, IHasId<TId>
            where TDtoHandler : class, IDtoHandler<TDto>
        {
            services.AddSingleton<Func<TId, Task<TDtoHandler>>>(sp => id => resolver.Invoke(sp, id));
            return services;
        }

         
    }
}