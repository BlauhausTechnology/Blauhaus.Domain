using System;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.Builders.Base;
using Moq;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockModelOwnerExtensions
    {
        public static Mock<TModelOwner> Where_GetDtoAsync_returns<TModelOwner, TDto>(this Mock<TModelOwner> mock, TDto model) 
            where TModelOwner : class, IModelOwner<TDto> 
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetModelAsync()).ReturnsAsync(model);
            return mock;
        }

        public static Mock<TModelOwner> Where_GetDtoAsync_returns<TModelOwner, TDto>(this Mock<TModelOwner> mock, Func<TDto> model) 
            where TModelOwner : class, IModelOwner<TDto> 
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetModelAsync()).ReturnsAsync(model);
            return mock;
        }

        public static Mock<TModelOwner> Where_GetDtoAsync_returns_object<TModelOwner, TDto>(this Mock<TModelOwner> mock, IBuilder<TDto> modelBuilder) 
            where TModelOwner : class, IModelOwner<TDto>  
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetModelAsync())
                .ReturnsAsync(()=> modelBuilder.Object);
            return mock;
        }
    }
}