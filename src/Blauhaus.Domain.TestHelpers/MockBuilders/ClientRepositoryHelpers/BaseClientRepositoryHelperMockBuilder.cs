using System.Collections.Generic;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SQLite;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.ClientRepositoryHelpers
{
    public class ClientRepositoryHelperMockBuilder<TModel, TRootEntity, TDto> : BaseClientRepositoryHelperMockBuilder<ClientRepositoryHelperMockBuilder<TModel, TRootEntity, TDto>, IClientRepositoryHelper<TModel, TRootEntity, TDto>, TModel, TRootEntity, TDto>
    {

    }


    public abstract class BaseClientRepositoryHelperMockBuilder<TBuilder, TMock, TModel, TRootEntity, TDto> 
        : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IClientRepositoryHelper<TModel, TRootEntity, TDto>
        where TBuilder : BaseClientRepositoryHelperMockBuilder<TBuilder, TMock, TModel, TRootEntity, TDto>
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