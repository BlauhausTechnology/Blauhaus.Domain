using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Abstractions.Entities;
using System;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public class ClientRepository<TModel, TDto, TRootEntity> : BaseClientRepository<TModel, TDto, TRootEntity> 
        where TModel : class, IClientEntity<Guid> 
        where TRootEntity : ISyncClientEntity<Guid>, new()
    {
        public ClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityConverter<TModel, TDto, TRootEntity> clientEntityConverter) 
                : base(analyticsService, sqliteDatabaseService, clientEntityConverter)
        {
        }
    }
}