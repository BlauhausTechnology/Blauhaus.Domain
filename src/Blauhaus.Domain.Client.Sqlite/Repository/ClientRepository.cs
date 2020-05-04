using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.ClientDatabase.Sqlite.Entities;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public class ClientRepository<TModel, TDto, TRootEntity> : BaseClientRepository<TModel, TDto, TRootEntity> 
        where TModel : class, IClientEntity 
        where TRootEntity : BaseSqliteEntity, new()
    {
        public ClientRepository(
            IAnalyticsService analyticsService,
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientEntityManager<TModel, TDto, TRootEntity> helper) 
                : base(analyticsService, sqliteDatabaseService, helper)
        {
        }
    }
}