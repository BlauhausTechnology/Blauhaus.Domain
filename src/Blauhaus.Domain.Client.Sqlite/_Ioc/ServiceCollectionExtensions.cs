using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Client.Sqlite._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientRepository<TModel, TDto, TRootEntity, TEntityConverter>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRootEntity : BaseSyncClientEntity, new() 
            where TEntityConverter : class, IClientEntityConverter<TModel, TDto, TRootEntity>
        {
            services.AddTransient<IClientRepository<TModel, TDto>, ClientRepository<TModel, TDto, TRootEntity>>();
            services.AddTransient<IClientEntityConverter<TModel, TDto, TRootEntity>, TEntityConverter>();
            return services;
        }
        public static IServiceCollection AddSyncClientRepository<TModel, TDto, TRootEntity, TEntityConverter>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRootEntity : BaseSyncClientEntity, new() 
            where TEntityConverter : class, IClientEntityConverter<TModel, TDto, TRootEntity>
        {
            services.AddTransient<ISyncClientRepository<TModel, TDto, SyncCommand>, SyncClientRepository<TModel, TDto, SyncCommand, TRootEntity>>();
            services.AddTransient<IClientEntityConverter<TModel, TDto, TRootEntity>, TEntityConverter>();
            services.AddTransient<ISyncClientSqlQueryGenerator<SyncCommand, TRootEntity>, SyncClientSqlQueryGenerator<TRootEntity>>();
            return services;
        }

        public static IServiceCollection AddSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity, TEntityConverter, TSqlQueryGenerator>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRootEntity : BaseSyncClientEntity, new() 
            where TEntityConverter : class, IClientEntityConverter<TModel, TDto, TRootEntity>
            where TSyncCommand : SyncCommand
            where TSqlQueryGenerator : class, ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>
        {
            services.AddTransient<ISyncClientRepository<TModel, TDto, TSyncCommand>, SyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity>>();
            services.AddTransient<IClientEntityConverter<TModel, TDto, TRootEntity>, TEntityConverter>();
            services.AddTransient<ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>, TSqlQueryGenerator>();
            return services;
        }
    }
}