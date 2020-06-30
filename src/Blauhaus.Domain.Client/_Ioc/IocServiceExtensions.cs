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
    public static class IocServiceExtensions
    {

        public static IIocService AddEntityClientCommandHandler<TModel, TModelDto, TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IIocService iocService) 
            where TModel : class, IClientEntity 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, ICommandHandler<TModelDto, TCommandDto>
        {
            iocService.RegisterImplementation<ICommandHandler<TModel, TCommand>, EntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand>>();
            iocService.RegisterImplementation<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            iocService.RegisterImplementation<ICommandHandler<TModelDto, TCommandDto>, TDtoCommandHandler>();
            return iocService;
        }
        
        public static IIocService AddVoidClientCommandHandler<TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IIocService iocService) 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, IVoidCommandHandler<TCommandDto>
        {
            iocService.RegisterImplementation<IVoidCommandHandler<TCommand>, VoidCommandClientHandler<TCommandDto, TCommand>>();
            iocService.RegisterImplementation<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            iocService.RegisterImplementation<IVoidCommandHandler<TCommandDto>, TDtoCommandHandler>();
            return iocService;
        }
        
        
        public static IIocService AddSyncService<TSyncCommand, TSyncClientFactory>(this IIocService iocService) 
            where TSyncCommand : SyncCommand, new()
            where TSyncClientFactory : class, ISyncClientFactory<TSyncCommand>
        {
            iocService.RegisterImplementation<ISyncService, SyncService<TSyncCommand>>();
            iocService.RegisterImplementation<ISyncClientFactory<TSyncCommand>, TSyncClientFactory>();
            iocService.RegisterImplementation<ISyncStatusHandlerFactory, SyncstatusHandlerFactory>();
            return iocService;
        }

        
        public static IIocService AddSyncCollection<TModel, TViewElement, TSyncCommand, TUpdater>(this IIocService iocService) 
            where TModel : class, IClientEntity 
            where TViewElement : ListItem, new() 
            where TSyncCommand : SyncCommand, new()
            where TUpdater : class, IListItemUpdater<TModel, TViewElement>
        {
            iocService.RegisterType<SyncCollection<TModel, TViewElement, TSyncCommand>>();
            iocService.RegisterImplementation<IListItemUpdater<TModel, TViewElement>, TUpdater>();
            return iocService;
        }
    }
}