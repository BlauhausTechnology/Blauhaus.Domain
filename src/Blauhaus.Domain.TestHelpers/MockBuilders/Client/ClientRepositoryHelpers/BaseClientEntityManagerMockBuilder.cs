using System;
using System.Collections.Generic;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SQLite;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientRepositoryHelpers
{
    public class ClientEntityConverterMockBuilder<TModel, TDto, TRootEntity> : BaseClientEntityManagerMockBuilder<ClientEntityConverterMockBuilder<TModel, TDto, TRootEntity>, IClientEntityConverter<TModel, TDto, TRootEntity>, TModel, TDto, TRootEntity> 
        where TModel : IClientEntity 
        where TRootEntity : ISyncClientEntity, new()
    {

    }


    public abstract class BaseClientEntityManagerMockBuilder<TBuilder, TMock, TModel, TDto, TRootEntity> 
        : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IClientEntityConverter<TModel, TDto, TRootEntity>
        where TBuilder : BaseClientEntityManagerMockBuilder<TBuilder, TMock, TModel, TDto, TRootEntity>
        where TModel : IClientEntity
        where TRootEntity : ISyncClientEntity, new()
    {

        public TBuilder Where_ConstructModel_returns(TModel value)
        {
            Mock.Setup(x => x.ConstructModel(It.IsAny<TRootEntity>(), It.IsAny<IEnumerable<ISyncClientEntity>>()))
                .Returns(value);
            return (TBuilder) this;
        }
        
        public TBuilder Where_ExtractEntitiesFromDto_returns_root(TRootEntity value)
        {
            Mock.Setup(x => x.ExtractEntitiesFromDto(It.IsAny<TDto>()))
                .Returns(new Tuple<TRootEntity, List<ISyncClientEntity>>(value, new List<ISyncClientEntity>()));
            return (TBuilder) this;
        }
        public TBuilder Where_ExtractEntitiesFromDto_returns(TRootEntity value, List<ISyncClientEntity> childEntities)
        {
            Mock.Setup(x => x.ExtractEntitiesFromDto(It.IsAny<TDto>()))
                .Returns(new Tuple<TRootEntity, List<ISyncClientEntity>>(value, childEntities));
            return (TBuilder) this;
        }
        public TBuilder Where_ExtractEntitiesFromDto_returns(Tuple<TRootEntity,List<ISyncClientEntity>> value)
        {
            Mock.Setup(x => x.ExtractEntitiesFromDto(It.IsAny<TDto>()))
                .Returns(value);
            return (TBuilder) this;
        }
          
        public TBuilder Where_LoadChildEntities_returns(List<ISyncClientEntity> value)
        {
            Mock.Setup(x => x.LoadChildEntities(It.IsAny<TRootEntity>(), It.IsAny<SQLiteConnection>()))
                .Returns(value);
            return (TBuilder) this;
        }


    }
}