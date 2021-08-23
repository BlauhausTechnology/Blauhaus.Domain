using System;
using System.Linq.Expressions;
using Blauhaus.Domain.Abstractions.Repositories;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.Repositories
{ 
    public class ClientRepositoryMockBuilder<TModel, TDto> : BaseClientRepositoryMockBuilder<ClientRepositoryMockBuilder<TModel, TDto>, IClientRepository<TModel, TDto>, TModel, TDto>
        where TModel : class, IClientEntity
    {

    }


    public abstract class BaseClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto> : BaseMockBuilder<TBuilder, TMock> 
        where TBuilder : BaseClientRepositoryMockBuilder<TBuilder, TMock, TModel, TDto>
        where TMock : class, IClientRepository<TModel, TDto>
        where TModel : class, IClientEntity
    {
        
        public TBuilder Where_LoadByIdAsync_returns(TModel model, Guid id)
        {
            Mock.Setup(x => x.LoadByIdAsync(id))
                .ReturnsAsync(model);
            return (TBuilder)this;
        }

        public TBuilder Where_LoadByIdAsync_returns(TModel model)
        {
            Mock.Setup(x => x.LoadByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(model);
            return (TBuilder)this;
        }
        
        public TBuilder Where_LoadByIdAsync_throws(Exception e)
        {
            Mock.Setup(x => x.LoadByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(e);
            return (TBuilder)this;
        }

        public void Verify_LoadAsync(Guid id)
        {
            Mock.Verify(x => x.LoadByIdAsync(id));
        }

        public TBuilder Where_SaveDtoAsync_returns(TModel userModel)
        {
            Mock.Setup(x => x.SaveDtoAsync(It.IsAny<TDto>()))
                .ReturnsAsync(userModel);
            return (TBuilder)this;
        }
        public TBuilder Where_SaveDtoAsync_throws(Exception e)
        {
            Mock.Setup(x => x.SaveDtoAsync(It.IsAny<TDto>()))
                .ThrowsAsync(e);
            return (TBuilder)this;
        }
        public void VerifySaveDtoAsync(params Expression<Func<TDto, bool>>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                Mock.Verify(x => x.SaveDtoAsync(It.Is(predicate)));
            }
        }       
        
        public void VerifySaveDtoAsync(TDto dto)
        {
            Mock.Verify(x => x.SaveDtoAsync(dto));
        }
    
    }

}