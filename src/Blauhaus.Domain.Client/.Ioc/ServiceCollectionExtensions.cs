using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Client.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;
using System;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Client.Sync.Old.Client;
using Blauhaus.Domain.Client.Sync.Old.Collection;
using Blauhaus.Domain.Client.Sync.Old.Model;
using Blauhaus.Domain.Client.Sync.Old.Service;

namespace Blauhaus.Domain.Client.Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDtoSyncHandler<TDto, TId>(this IServiceCollection services) 
            where TDto : class, IClientEntity<TId> 
            where TId : IEquatable<TId>
        {
            services.AddSingleton<IDtoSyncHandler, DtoSyncHandler<TDto, TId>>();

            return services;
        }


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

        public static IServiceCollection AddEntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IServiceCollection services)
            where TModel : class, IClientEntity<Guid>
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
            where TModel : class, IClientEntity <Guid>
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
            where TModel : class, IClientEntity <Guid>
            where TSyncCommand : SyncCommand, new()
        {
            services.TryAddTransient<ISyncStatusHandler, SyncStatusHandler>();
            services.AddTransient<ISyncModel<TModel>, SyncModel<TModel, TSyncCommand>>(); 
            return services;
        }
         
    }
}