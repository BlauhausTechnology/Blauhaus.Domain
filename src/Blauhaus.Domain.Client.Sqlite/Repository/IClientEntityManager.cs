using System.Collections.Generic;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Common.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public interface IClientEntityManager<TModel, TDto, TRootEntity> 
        where TModel : IClientEntity
        where TRootEntity: ISyncClientEntity
    {
        TRootEntity ExtractRootEntityFromDto(TDto dto);
        IEnumerable<ISyncClientEntity> ExtractChildEntitiesFromDto(TDto dto);
        TModel ConstructModelFromRootEntity(TRootEntity rootEntity, SQLiteConnection conn);
    }
}