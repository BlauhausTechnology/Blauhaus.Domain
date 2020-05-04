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
        public static IServiceCollection AddClientRepository<TModel, TDto, TRootEntity, TEntityManager>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRootEntity : BaseSyncClientEntity, new() 
            where TEntityManager : class, IClientEntityManager<TModel, TDto, TRootEntity>
        {
            services.AddTransient<IClientRepository<TModel, TDto>, ClientRepository<TModel, TDto, TRootEntity>>();
            services.AddTransient<IClientEntityManager<TModel, TDto, TRootEntity>, TEntityManager>();
            return services;
        }
        public static IServiceCollection AddSyncClientRepository<TModel, TDto, TRootEntity, TEntityManager>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRootEntity : BaseSyncClientEntity, new() 
            where TEntityManager : class, IClientEntityManager<TModel, TDto, TRootEntity>
        {
            services.AddTransient<ISyncClientRepository<TModel, TDto, SyncCommand>, SyncClientRepository<TModel, TDto, SyncCommand, TRootEntity>>();
            services.AddTransient<IClientEntityManager<TModel, TDto, TRootEntity>, TEntityManager>();
            services.AddTransient<ISyncQueryGenerator<SyncCommand>, SyncQueryGenerator>();
            return services;
        }

        public static IServiceCollection AddSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity, TEntityManager, TSyncQueryGenerator>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TRootEntity : BaseSyncClientEntity, new() 
            where TEntityManager : class, IClientEntityManager<TModel, TDto, TRootEntity>
            where TSyncCommand : SyncCommand
            where TSyncQueryGenerator : class, ISyncQueryGenerator<TSyncCommand>
        {
            services.AddTransient<ISyncClientRepository<TModel, TDto, TSyncCommand>, SyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity>>();
            services.AddTransient<IClientEntityManager<TModel, TDto, TRootEntity>, TEntityManager>();
            services.AddTransient<ISyncQueryGenerator<TSyncCommand>, TSyncQueryGenerator>();
            return services;
        }
    }
}