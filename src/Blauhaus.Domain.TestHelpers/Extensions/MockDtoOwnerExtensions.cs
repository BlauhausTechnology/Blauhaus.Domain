using System;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.Builders.Base;
using Moq;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockDtoOwnerExtensions
    {
        public static Mock<TDtoOwner> Where_GetDtoAsync_returns<TDtoOwner, TDto>(this Mock<TDtoOwner> mock, TDto dto) 
            where TDtoOwner : class, IDtoOwner<TDto> 
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetDtoAsync()).ReturnsAsync(dto);
            return mock;
        }

        public static Mock<TDtoOwner> Where_GetDtoAsync_returns<TDtoOwner, TDto>(this Mock<TDtoOwner> mock, Func<TDto> dtoFunc) 
            where TDtoOwner : class, IDtoOwner<TDto> 
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetDtoAsync()).ReturnsAsync(dtoFunc);
            return mock;
        }

        public static Mock<TDtoOwner> Where_GetDtoAsync_returns_object<TDtoOwner, TDto>(this Mock<TDtoOwner> mock, IBuilder<TDto> dtoBuilder) 
            where TDtoOwner : class, IDtoOwner<TDto>  
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetDtoAsync())
                .ReturnsAsync(()=> dtoBuilder.Object);
            return mock;
        }
    }
}