﻿using Blauhaus.Domain.Client.CommandHandlers;
using Blauhaus.Domain.Client.CommandHandlers.Entities;
using Blauhaus.Domain.Client.Entities;
using Blauhaus.Domain.Client.Repositories;
using Blauhaus.Domain.Common.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Client._Ioc
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddEntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IServiceCollection services) 
            where TModel : class, IClientEntity 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, ICommandHandler<TModelDto, TCommandDto>
        {
            services.AddTransient<ICommandHandler<TModel, TCommand>, EntityCommandClientHandler<TModel, TModelDto, TCommandDto, TCommand>>();
            services.AddTransient<ICommandConverter<TCommandDto, TCommand>, TCommandConverter>();
            services.AddTransient<ICommandHandler<TModelDto, TCommandDto>, TDtoCommandHandler>();
            return services;
        }

        public static IServiceCollection AddVoidCommandClientHandler<TCommandDto, TCommand, TCommandConverter, TDtoCommandHandler>(this IServiceCollection services) 
            where TCommandConverter : class, ICommandConverter<TCommandDto, TCommand>
            where TDtoCommandHandler : class, IVoidCommandHandler<TCommandDto>
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
    }
}