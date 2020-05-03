using Blauhaus.ClientDatabase.Sqlite.Entities;
using Blauhaus.ClientDatabase.Sqlite.Service;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public class ClientRepository<TModel, TRootEntity, TDto> : BaseSqliteRepository<TModel, TRootEntity, TDto> 
        where TModel : class, IClientEntity 
        where TRootEntity : BaseSqliteEntity, new()
    {
        public ClientRepository(
            ISqliteDatabaseService sqliteDatabaseService, 
            IClientRepositoryHelper<TModel, TRootEntity, TDto> helper) 
                : base(sqliteDatabaseService, helper)
        {
        }
    }
}