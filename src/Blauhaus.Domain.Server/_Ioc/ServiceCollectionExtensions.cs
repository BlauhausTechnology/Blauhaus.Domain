using Blauhaus.Domain.Common.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Server._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticatedCommandHandler<TPayload, TCommand, TUser, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IAuthenticatedCommandHandler<TPayload, TCommand, TUser>
        {
            services.AddScoped<IAuthenticatedCommandHandler<TPayload, TCommand, TUser>, TCommandHandler>();
            return services;
        }
        public static IServiceCollection AddCommandHandler<TPayload, TCommand, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, ICommandHandler<TPayload, TCommand>
        {
            services.AddScoped<ICommandHandler<TPayload, TCommand>, TCommandHandler>();
            return services;
        }

        public static IServiceCollection AddVoidCommandHandler<TCommand, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IVoidCommandHandler<TCommand>
        {
            services.AddScoped<IVoidCommandHandler<TCommand>, TCommandHandler>();
            return services;
        }
        
        public static IServiceCollection AddVoidAuthenticatedCommandHandler<TCommand, TUser, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IVoidAuthenticatedCommandHandler<TCommand, TUser>
        {
            services.AddScoped<IVoidAuthenticatedCommandHandler<TCommand, TUser>, TCommandHandler>();
            return services;
        }

    }
}