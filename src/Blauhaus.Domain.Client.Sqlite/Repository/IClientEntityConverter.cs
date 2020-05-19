using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Common.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public interface IClientEntityConverter<TModel, TDto, TRootEntity> 
        where TModel : IClientEntity
        where TRootEntity: ISyncClientEntity
    {
        Tuple<TRootEntity, List<ISyncClientEntity>> ExtractEntitiesFromDto(TDto dto);
        TModel ConstructModel(TRootEntity rootEntity, IEnumerable<ISyncClientEntity> childEntities);
        IEnumerable<ISyncClientEntity> LoadChildEntities(TRootEntity rootEntity, SQLiteConnection conn);
    }
}