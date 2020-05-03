using System.Collections.Generic;
using Blauhaus.Domain.Common.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public interface IClientRepositoryHelper<TModel, TRootEntity, TDto>
    {
        TRootEntity ExtractRootEntityFromDto(TDto dto);
        IEnumerable<IClientEntity> ExtractChildEntitiesFromDto(TDto dto);
        TModel ConstructModelFromRootEntity(TRootEntity rootEntity, SQLiteConnection conn);
    }
}