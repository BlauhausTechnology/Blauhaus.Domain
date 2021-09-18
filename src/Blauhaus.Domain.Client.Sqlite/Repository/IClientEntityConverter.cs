using System;
using System.Collections.Generic;
using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Repository
{
    public interface IDtoConverter<TDto, TRootEntity> : IClientEntityConverter<TDto, TDto, TRootEntity>
        where TRootEntity: ISyncClientEntity <Guid>
        where TDto : IClientEntity<Guid>
    {
    }


    public interface IClientEntityConverter<out TModel, in TDto, TRootEntity>
        where TModel : IClientEntity<Guid>
        where TRootEntity: ISyncClientEntity<Guid>
    {
        TModel ConstructModel(TRootEntity rootEntity, List<ISyncClientEntity<Guid>> childEntities);
        Tuple<TRootEntity, List<ISyncClientEntity<Guid>>> ExtractEntitiesFromDto(TDto dto);
        List<ISyncClientEntity<Guid>> LoadChildEntities(TRootEntity rootEntity, SQLiteConnection conn);
    }
}