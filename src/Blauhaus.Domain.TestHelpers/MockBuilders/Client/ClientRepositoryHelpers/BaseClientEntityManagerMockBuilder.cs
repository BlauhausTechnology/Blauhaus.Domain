﻿using System.Collections.Generic;
using Blauhaus.Domain.Client.Sqlite.Repository;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using SQLite;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.ClientRepositoryHelpers
{
    public class ClientEntityManagerMockBuilder<TModel, TDto, TRootEntity> : BaseClientEntityManagerMockBuilder<ClientEntityManagerMockBuilder<TModel, TDto, TRootEntity>, IClientEntityConverter<TModel, TDto, TRootEntity>, TModel, TDto, TRootEntity> 
        where TModel : IClientEntity 
        where TRootEntity : ISyncClientEntity
    {

    }


    public abstract class BaseClientEntityManagerMockBuilder<TBuilder, TMock, TModel, TDto, TRootEntity> 
        : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IClientEntityConverter<TModel, TDto, TRootEntity>
        where TBuilder : BaseClientEntityManagerMockBuilder<TBuilder, TMock, TModel, TDto, TRootEntity>
        where TModel : IClientEntity
        where TRootEntity : ISyncClientEntity
    {

        public TBuilder Where_ConstructModelFromRootEntity_returns(TModel value)
        {
            Mock.Setup(x => x.ConstructModelFromRootEntity(It.IsAny<TRootEntity>(), It.IsAny<SQLiteConnection>()))
                .Returns(value);
            return this as TBuilder;
        }
        
        public TBuilder Where_ExtractChildEntitiesFromDto_returns(IEnumerable<ISyncClientEntity> value)
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