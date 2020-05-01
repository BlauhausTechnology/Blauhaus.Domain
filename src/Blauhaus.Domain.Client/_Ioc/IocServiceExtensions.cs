using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Ioc.Abstractions;

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
    }
}