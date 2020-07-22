using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Collection;
using Blauhaus.Domain.Client.Sync.Model;
using Blauhaus.Domain.Client.Sync.Service;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Ioc.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Client._Ioc
{
    public static class IocServiceExtensions
    {

        public static IIocService AddEntityClientCommandHandler<TModel, TModelDto, TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IIocService iocService) 
            where TModel : class, IClientEntity 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, ICommandHandler<TModelDto, TCommandDto>
            where TCommandDto : notnull
            where TCommand : notnull
        {
            
            iocService.RegisterImplementation<ICommandHandler<TModel, TCommand>, EntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand>>(IocLifetime.Singleton);
            iocService.RegisterImplementation<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            iocService.RegisterImplementation<ICommandHandler<TModelDto, TCommandDto>, TDtoCommandHandler>();
            return iocService;
        }
        
        public static IIocService AddVoidClientCommandHandler<TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IIocService iocService) 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, IVoidCommandHandler<TCommandDto>
            where TCommandDto : notnull
            where TCommand : notnull
        {
            iocService.RegisterImplementation<IVoidCommandHandler<TCommand>, VoidCommandClientHandler<TCommandDto, TCommand>>(IocLifetime.Singleton);
            iocService.RegisterImplementation<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            iocService.RegisterImplementation<IVoidCommandHandler<TCommandDto>, TDtoCommandHandler>();
            return iocService;
        }
        
        
        public static IIocService AddSyncService<TSyncCommand, TSyncClientFactory>(this IIocService iocService) 
            where TSyncCommand : SyncCommand, new()
            where TSyncClientFactory : class, ISyncClientFactory<TSyncCommand>
        {
            iocService.RegisterImplementation<ISyncStatusHandler, SyncStatusHandler>();
            iocService.RegisterImplementation<ISyncService, SyncService<TSyncCommand>>();
            iocService.RegisterImplementation<ISyncClientFactory<TSyncCommand>, TSyncClientFactory>();
            iocService.RegisterImplementation<ISyncStatusHandlerFactory, SyncstatusHandlerFactory>();
            return iocService;
        }

        public static IIocService AddSyncCollection<TModel, TListItem, TSyncCommand>(this IIocService iocService) 
            where TModel : class, IClientEntity 
            where TListItem : class, IListItem<TModel>
            where TSyncCommand : SyncCommand, new()
        {
            iocService.RegisterImplementation<ISyncCollection<TModel, TListItem, TSyncCommand>, SyncCollection<TModel, TListItem, TSyncCommand>>(); 
            iocService.RegisterType<TListItem>(); 
            return iocService;
        }
         
        public static IIocService AddSyncModel<TModel, TSyncCommand>(this IIocService iocService) 
            where TModel : class, IClientEntity 
            where TSyncCommand : SyncCommand, new()
        {
            iocService.RegisterImplementation<ISyncStatusHandler, SyncStatusHandler>();
            iocService.RegisterImplementation<ISyncModel<TModel>, SyncModel<TModel, TSyncCommand>>(); 
            return iocService;
        }

         
    }
}