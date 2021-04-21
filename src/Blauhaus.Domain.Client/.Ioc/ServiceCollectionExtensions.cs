using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Client.Sync.Model;
using Blauhaus.Domain.Client.Sync.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blauhaus.Domain.Client.Ioc
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

            services.AddScoped<ICommandHandler<TModel, TCommand>, EntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand>>();
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
            services.AddScoped<IVoidCommandHandler<TCommand>, VoidCommandClientHandler<TCommandDto, TCommand>>();
            services.AddTransient<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            services.AddTransient<IVoidCommandHandler<TCommandDto>, TDtoCommandHandler>();
            return services;
        }
         
        
        public static IServiceCollection AddSyncService<TSyncCommand, TSyncClientFactory>(this IServiceCollection services) 
            where TSyncCommand : SyncCommand, new()
            where TSyncClientFactory : class, ISyncClientFactory<TSyncCommand>
        {
            services.TryAddTransient<ISyncStatusHandler, SyncStatusHandler>();
            services.AddTransient<ISyncService, SyncService<TSyncCommand>>();
            services.AddTransient<ISyncClientFactory<TSyncCommand>, TSyncClientFactory>();
            services.AddTransient<ISyncStatusHandler, SyncStatusHandler>();
            return services;
        }
         
        
        public static IServiceCollection AddSyncCollection<TModel, TListItem, TSyncCommand>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TListItem : class, IListItem<TModel>
            where TSyncCommand : SyncCommand, new()
        {
            services.TryAddTransient<ISyncStatusHandler, SyncStatusHandler>();
            services.AddTransient<ISyncCollection<TModel, TListItem, TSyncCommand>, SyncCollection<TModel, TListItem, TSyncCommand>>(); 
            services.AddTransient<SyncCollection<TModel, TListItem, TSyncCommand>>(); 
            services.AddTransient<TListItem>(); 
            return services;
        }
         
        public static IServiceCollection AddSyncModel<TModel, TSyncCommand>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TSyncCommand : SyncCommand, new()
        {
            services.TryAddTransient<ISyncStatusHandler, SyncStatusHandler>();
            services.AddTransient<ISyncModel<TModel>, SyncModel<TModel, TSyncCommand>>(); 
            return services;
        }
         
    }
}