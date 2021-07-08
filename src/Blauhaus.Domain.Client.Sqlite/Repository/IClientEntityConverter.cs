using System;
using System.Collections.Generic;
using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public interface IDtoConverter<TDto, TRootEntity> : IClientEntityConverter<TDto, TDto, TRootEntity>
        where TRootEntity: ISyncClientEntity 
        where TDto : IClientEntity
    {
    }


    public interface IClientEntityConverter<out TModel, in TDto, TRootEntity>
        where TModel : IClientEntity
        where TRootEntity: ISyncClientEntity
    {
        TModel ConstructModel(TRootEntity rootEntity, List<ISyncClientEntity> childEntities);
        Tuple<TRootEntity, List<ISyncClientEntity>> ExtractEntitiesFromDto(TDto dto);
        List<ISyncClientEntity> LoadChildEntities(TRootEntity rootEntity, SQLiteConnection conn);
    }
}