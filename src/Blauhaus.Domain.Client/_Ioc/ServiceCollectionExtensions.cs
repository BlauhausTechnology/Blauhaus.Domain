using System;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Client.Sync.Service;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Ioc.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Client._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IServiceCollection services)
            where TModel : class, IClientEntity
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, ICommandHandler<TModelDto, TCommandDto>
            where TCommand : notnull
            where TCommandDto : notnull
        {

            services.AddTransient<ICommandHandler<TModel, TCommand>, EntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand>>();
            services.AddTransient<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            services.AddTransient<ICommandHandler<TModelDto, TCommandDto>, TDtoCommandHandler>();
            return services;
        }

        public static IServiceCollection AddVoidCommandClientHandler<TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IServiceCollection services) 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, IVoidCommandHandler<TCommandDto>
            where TCommand : notnull
            where TCommandDto : notnull
        {
            services.AddTransient<IVoidCommandHandler<TCommand>, VoidCommandClientHandler<TCommandDto, TCommand>>();
            services.AddTransient<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            services.AddTransient<IVoidCommandHandler<TCommandDto>, TDtoCommandHandler>();
            return services;
        }
         
        
        public static IServiceCollection AddSyncService<TSyncCommand, TSyncClientFactory>(this IServiceCollection services) 
            where TSyncCommand : SyncCommand, new()
            where TSyncClientFactory : class, ISyncClientFactory<TSyncCommand>
        {
            services.AddTransient<ISyncService, SyncService<TSyncCommand>>();
            services.AddTransient<ISyncClientFactory<TSyncCommand>, TSyncClientFactory>();
            services.AddTransient<ISyncStatusHandlerFactory, SyncstatusHandlerFactory>();
            return services;
        }
         
        
        public static IServiceCollection AddSyncCollection<TModel, TListItem, TSyncCommand>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TListItem : class, IListItem<TModel>, new() 
            where TSyncCommand : SyncCommand, new()
        {
            services.AddTransient<SyncCollection<TModel, TListItem, TSyncCommand>>(); 
            services.AddTransient<IListItem<TModel>, TListItem>(); 
            return services;
        }
         
    }
}