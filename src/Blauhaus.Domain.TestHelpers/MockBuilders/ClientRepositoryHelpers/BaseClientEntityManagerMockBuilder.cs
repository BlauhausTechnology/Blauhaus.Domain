using System.Collections.Generic;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SQLite;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.ClientRepositoryHelpers
{
    public class ClientEntityManagerMockBuilder<TModel, TDto, TRootEntity> 
        : BaseClientEntityManagerMockBuilder<ClientEntityManagerMockBuilder<TModel, TDto, TRootEntity>, IClientEntityManager<TModel, TDto, TRootEntity>, TModel, TDto, TRootEntity>
    {

    }


    public abstract class BaseClientEntityManagerMockBuilder<TBuilder, TMock, TModel, TDto, TRootEntity> 
        : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IClientEntityManager<TModel, TDto, TRootEntity>
        where TBuilder : BaseClientEntityManagerMockBuilder<TBuilder, TMock, TModel, TDto, TRootEntity>
    {

        public TBuilder Where_ConstructModelFromRootEntity_returns(TModel value)
        {
            Mock.Setup(x => x.ConstructModelFromRootEntity(It.IsAny<TRootEntity>(), It.IsAny<SQLiteConnection>()))
                .Returns(value);
            return this as TBuilder;
        }
        
        public TBuilder Where_ExtractChildEntitiesFromDto_returns(IEnumerable<IClientEntity> value)
        {
            Mock.Setup(x => x.ExtractChildEntitiesFromDto(It.IsAny<TDto>()))
                .Returns(value);
            return this as TBuilder;
        }

        
        public TBuilder Where_ExtractRootEntityFromDto_returns(TRootEntity value)
        {
            Mock.Setup(x => x.ExtractRootEntityFromDto(It.IsAny<TDto>()))
                .Returns(value);
            return this as TBuilder;
        }


    }
}