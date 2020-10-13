using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public class ClientRepository<TModel, TDto, TRootEntity> : BaseClientRepository<TModel, TDto, TRootEntity> 
        where TModel : class, IClientEntity 
        where TRootEntity : ISyncClientEntity, new()
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