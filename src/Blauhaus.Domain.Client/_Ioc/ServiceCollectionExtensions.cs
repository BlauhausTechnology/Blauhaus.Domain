using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.CommandHandlers.Sync;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
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

        public static IServiceCollection AddClientRepository<TModel, TModelDto, TRepository>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRepository : class, IClientRepository<TModel, TModelDto>
        {
            services.AddTransient<IClientRepository<TModel, TModelDto>, TRepository>();
            return services;
        }

        public static IServiceCollection AddSyncClient<TModel, TModelDto, TSyncCommandDto, TSyncCommand, TRepository>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRepository : class, ISyncClientRepository<TModel, TModelDto>
            where TSyncCommand : SyncCommand
            where TSyncCommandDto :  notnull
        {
            services.AddScoped<ISyncClient<TModel, TSyncCommand>>();
            services.AddTransient<ICommandHandler<SyncResult<TModel>, TSyncCommand>, SyncCommandClientHandler<TModel, TModelDto, TSyncCommandDto, TSyncCommand>>();
            services.AddTransient<ISyncClientRepository<TModel, TModelDto>, TRepository>();
            return services;
        }

        
         
    }
}