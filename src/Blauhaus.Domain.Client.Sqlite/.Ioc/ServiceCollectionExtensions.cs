using System;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.DtoCaches;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Abstractions.Sync.Old;
using Blauhaus.Domain.Client.Ioc;
using Blauhaus.Domain.Client.Sqlite.DtoCaches;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Client.Sqlite.SyncRepository;
using Blauhaus.Domain.Client.Sync;
using Blauhaus.Domain.Client.Sync.Old.Client;
using Blauhaus.Domain.Client.Sync.Old.CommandHandler;
using Blauhaus.Domain.Client.Sync.Old.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blauhaus.Domain.Client.Sqlite.Ioc
{
    public static class ServiceCollectionExtensions
    {
         public static IServiceCollection AddSyncDto<TDto, TId, TEntity>(this IServiceCollection services)
            where TDto : ClientEntity<TId>, new()
            where TEntity : SyncClientEntity<TId>, IEquatable<TEntity>, new()
            where TId : IEquatable<TId>
         {
             return services
                 .AddSyncDtoCache<TDto, TId, SyncDtoCache<TDto, TEntity, TId>>();
        }

         public static IServiceCollection AddSyncDtoCache<TDto, TId, TSyncDtoCache>(this IServiceCollection services)
             where TDto : ClientEntity<TId>, new()
             where TId : IEquatable<TId>
             where TSyncDtoCache : class, ISyncDtoCache<TDto, TId>
         {
             services.TryAddSingleton<ISyncDtoCache<TDto, TId>, TSyncDtoCache>();
             return services;
         }
         

        public static IServiceCollection AddClientRepository<TModel, TDto, TRootEntity, TEntityConverter>(this IServiceCollection services) 
            where TModel : class, IClientEntity<Guid> 
            where TRootEntity : ISyncClientEntity<Guid>, new() 
            where TEntityConverter : class, IClientEntityConverter<TModel, TDto, TRootEntity>
        {
            services.AddTransient<IClientRepository<TModel, TDto>, ClientRepository<TModel, TDto, TRootEntity>>();
            services.TryAddTransient<IClientRepository<TModel>, ClientRepository<TModel, TDto, TRootEntity>>();

            services.AddTransient<IClientEntityConverter<TModel, TDto, TRootEntity>, TEntityConverter>();
            return services;
        }
        public static IServiceCollection AddSyncClientRepository<TModel, TDto, TRootEntity, TEntityConverter>(this IServiceCollection services) 
            where TModel : class, IClientEntity, IClientEntity<Guid>
            where TRootEntity : ISyncClientEntity<Guid>, new() 
            where TEntityConverter : class, IClientEntityConverter<TModel, TDto, TRootEntity>
        {
            services.AddTransient<ISyncClientRepository<TModel, TDto, SyncCommand>, SyncClientRepository<TModel, TDto, SyncCommand, TRootEntity>>();
            services.AddTransient<IClientEntityConverter<TModel, TDto, TRootEntity>, TEntityConverter>();
            services.AddTransient<ISyncClientSqlQueryGenerator<SyncCommand, TRootEntity>, SyncClientSqlQueryGenerator<TRootEntity>>();
            return services;
        }

        public static IServiceCollection AddSyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity, TEntityConverter, TSqlQueryGenerator>(this IServiceCollection services)
            where TModel : class, IClientEntity, IClientEntity<Guid>
            where TRootEntity : ISyncClientEntity<Guid>, new()
            where TEntityConverter : class, IClientEntityConverter<TModel, TDto, TRootEntity>
            where TSyncCommand : SyncCommand
            where TSqlQueryGenerator : class, ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>
        {
            services.AddTransient<ISyncClientRepository<TModel, TDto, TSyncCommand>, SyncClientRepository<TModel, TDto, TSyncCommand, TRootEntity>>();
            services.AddTransient<IClientEntityConverter<TModel, TDto, TRootEntity>, TEntityConverter>();
            services.AddTransient<ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>, TSqlQueryGenerator>();
            return services;
        }

        public static IServiceCollection AddSyncClient<TModel, TModelDto, TSyncCommandDto, TSyncCommand, TRootEntity, TEntityConverter, TSqlQueryGenerator>(this IServiceCollection services) 
            where TModel : class, IClientEntity <Guid>
            where TRootEntity : ISyncClientEntity<Guid>, new() 
            where TEntityConverter : class, IClientEntityConverter<TModel, TModelDto, TRootEntity>
            where TSyncCommand : SyncCommand, new()
            where TSqlQueryGenerator : class, ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>
            where TSyncCommandDto : notnull
        {

            services.TryAddTransient<ISyncClientRepository<TModel, TModelDto, TSyncCommand>, SyncClientRepository<TModel, TModelDto, TSyncCommand, TRootEntity>>();
            services.TryAddTransient<IClientRepository<TModel, TModelDto>, SyncClientRepository<TModel, TModelDto, TSyncCommand, TRootEntity>>();
            services.TryAddTransient<IClientRepository<TModel>, SyncClientRepository<TModel, TModelDto, TSyncCommand, TRootEntity>>();
            
            services.TryAddTransient<IClientEntityConverter<TModel, TModelDto, TRootEntity>, TEntityConverter>();
            services.TryAddTransient<ISyncClientSqlQueryGenerator<TSyncCommand, TRootEntity>, TSqlQueryGenerator>();
            services.AddTransient<ICommandHandler<SyncResult<TModel>, TSyncCommand>, SyncCommandClientHandler<TModel, TModelDto, TSyncCommandDto, TSyncCommand>>();

            services.AddScoped<ISyncClient<TModel, TSyncCommand>, SyncClient<TModel, TModelDto, TSyncCommand>>();
            services.AddScoped<ISyncModel<TModel>, SyncModel<TModel, TSyncCommand>>();

            return services;
        }
    }
}