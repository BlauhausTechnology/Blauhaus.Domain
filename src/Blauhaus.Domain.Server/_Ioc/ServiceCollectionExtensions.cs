using System;
using System.Linq;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Domain.Server.CommandHandlers;
using Blauhaus.Domain.Server.CommandHandlers.Sync;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Server._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticatedCommandHandler<TPayload, TCommand, TUser, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IAuthenticatedCommandHandler<TPayload, TCommand, TUser>
            where TCommand : notnull
            where TUser : notnull
        {
            services.AddScoped<IAuthenticatedCommandHandler<TPayload, TCommand, TUser>, TCommandHandler>();
            return services;
        }
        public static IServiceCollection AddAuthenticatedUserCommandHandler<TPayload, TCommand, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IAuthenticatedCommandHandler<TPayload, TCommand, IAuthenticatedUser>
            where TCommand : notnull
        {
            services.AddScoped<IAuthenticatedCommandHandler<TPayload, TCommand, IAuthenticatedUser>, TCommandHandler>();
            return services;
        }


        public static IServiceCollection AddCommandHandler<TPayload, TCommand, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, ICommandHandler<TPayload, TCommand>
            where TCommand : notnull    
        {
            services.AddScoped<ICommandHandler<TPayload, TCommand>, TCommandHandler>();
            return services;
        }

        public static IServiceCollection AddVoidCommandHandler<TCommand, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IVoidCommandHandler<TCommand>
            where TCommand : notnull    
        {
            services.AddScoped<IVoidCommandHandler<TCommand>, TCommandHandler>();
            return services;
        }
        
        public static IServiceCollection AddVoidAuthenticatedCommandHandler<TCommand, TUser, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IVoidAuthenticatedCommandHandler<TCommand, TUser>
            where TCommand : notnull    
            where TUser : notnull    
        {
            services.AddScoped<IVoidAuthenticatedCommandHandler<TCommand, TUser>, TCommandHandler>();
            return services;
        }
        
        public static IServiceCollection AddVoidAuthenticatedUserCommandHandler<TCommand, TCommandHandler>(this IServiceCollection services) 
            where TCommandHandler : class, IVoidAuthenticatedCommandHandler<TCommand, IAuthenticatedUser>
            where TCommand : notnull      
        {
            services.AddScoped<IVoidAuthenticatedCommandHandler<TCommand, IAuthenticatedUser>, TCommandHandler>();
            return services;
        }


        //Sync

        public static IServiceCollection AddAuthenticatedSyncCommandHandler<TEntity, TSyncCommand, TUser>(this IServiceCollection services) 
            where TEntity : IServerEntity 
            where TSyncCommand : SyncCommand
            where TUser : notnull
        {
            services.AddAuthenticatedCommandHandler<SyncResult<TEntity>, TSyncCommand, TUser, 
                AuthenticatedSyncCommandHandler<TEntity, TSyncCommand, TUser>>();
            return services;
        }

        public static IServiceCollection AddAuthenticatedSyncCommandHandler<TEntity,TSyncQuery, TQueryLoader, TUser>(this IServiceCollection services) 
            where TEntity : IServerEntity 
            where TUser : class
            where TQueryLoader : class, IAuthenticatedSyncQueryLoader<TEntity, TSyncQuery, TUser>
            where TSyncQuery : SyncCommand
        {
            services.AddAuthenticatedCommandHandler<SyncResult<TEntity>, SyncCommand, TUser, AuthenticatedSyncCommandHandler<TEntity, SyncCommand, TUser>>();
            services.AddScoped<IAuthenticatedSyncQueryLoader<TEntity, TSyncQuery, TUser>, TQueryLoader>();
            return services;
        }
        
        public static IServiceCollection AddAuthenticatedUserSyncCommandHandler<TEntity, TSyncQuery, TQueryLoader>(this IServiceCollection services) 
            where TEntity : IServerEntity 
            where TQueryLoader : class, IAuthenticatedSyncQueryLoader<TEntity, TSyncQuery, IAuthenticatedUser>
            where TSyncQuery : SyncCommand
        {
            return services.AddAuthenticatedSyncCommandHandler<TEntity, TSyncQuery, TQueryLoader, IAuthenticatedUser>();
        }
         
        public static IServiceCollection AddAuthenticatedUserSyncCommandHandler<TEntity, TQueryLoader>(this IServiceCollection services) 
            where TEntity : IServerEntity 
            where TQueryLoader : class, IAuthenticatedSyncQueryLoader<TEntity, SyncCommand, IAuthenticatedUser>
        {
            return services.AddAuthenticatedSyncCommandHandler<TEntity, SyncCommand, TQueryLoader, IAuthenticatedUser>();
        }

    }
}