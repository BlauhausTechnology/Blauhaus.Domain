﻿using System;
using System.Linq.Expressions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base
{
    
    public class VoidAuthenticatedCommandHandlerMockBuilder<TMock, TCommand, TUser> 
        : VoidAuthenticatedCommandHandlerMockBuilder<VoidAuthenticatedCommandHandlerMockBuilder<TMock, TCommand, TUser>, TMock, TCommand, TUser>
        where TMock : class, IVoidAuthenticatedCommandHandler<TCommand, TUser>
    {
    }


    public class VoidAuthenticatedCommandHandlerMockBuilder<TBuilder, TMock, TCommand, TUser> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IVoidAuthenticatedCommandHandler<TCommand, TUser> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
    {
        public TBuilder Where_HandleAsync_returns_result(Response payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(),  It.IsAny<TUser>()))
                .ReturnsAsync(payload);
            return this as TBuilder;
        }

        public TBuilder Where_HandleAsync_returns_fail(string error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(Response.Failure(Error.Create(error)));
            return this as TBuilder;
        }
        
        public TBuilder Where_HandleAsync_returns_fail(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(Response.Failure(error));
            return this as TBuilder;
        }


        public TBuilder Where_HandleAsync_returns_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ThrowsAsync(exception);
            return this as TBuilder;
        }

        public void Verify_HandleAsync_called_With_Command(Expression<Func<TCommand, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.Is<TCommand>(predicate), It.IsAny<TUser>()));
        }

        public void Verify_HandleAsync_NOT_called_With_Command(Expression<Func<TCommand, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.Is<TCommand>(predicate), It.IsAny<TUser>()), Times.Never);
        }

        public void Verify_HandleAsync_called_With_User(Expression<Func<TUser, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>(), It.Is<TUser>(predicate)));
        }

        public void Verify_HandleAsync_NOT_called_With_User(Expression<Func<TUser, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>(), It.Is<TUser>(predicate)), Times.Never);
        }

        public void Verify_HandleAsync_NOT_called()
        {
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()), Times.Never);
        }
    }
}