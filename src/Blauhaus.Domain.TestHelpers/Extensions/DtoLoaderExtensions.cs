﻿using System;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.TestHelpers.Builders.Base;
using Moq;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class MockDtoLoaderExtensions
    {
        public static Mock<TDtoLoader> Where_GetDtoAsync_returns<TDtoLoader, TDto>(this Mock<TDtoLoader> mock, TDto dto) 
            where TDtoLoader : class, IDtoOwner<TDto> 
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetDtoAsync()).ReturnsAsync(dto);
            return mock;
        }

        public static Mock<TDtoLoader> Where_GetDtoAsync_returns<TDtoLoader, TDto>(this Mock<TDtoLoader> mock, Func<TDto> dtoFunc) 
            where TDtoLoader : class, IDtoOwner<TDto> 
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetDtoAsync()).ReturnsAsync(dtoFunc);
            return mock;
        }

        public static Mock<TDtoLoader> Where_GetDtoAsync_returns_object<TDtoLoader, TDto>(this Mock<TDtoLoader> mock, IBuilder<TDto> dtoBuilder) 
            where TDtoLoader : class, IDtoOwner<TDto>  
            where TDto : IClientEntity
        {
            mock.Setup(x => x.GetDtoAsync())
                .ReturnsAsync(()=> dtoBuilder.Object);
            return mock;
        }
    }
}