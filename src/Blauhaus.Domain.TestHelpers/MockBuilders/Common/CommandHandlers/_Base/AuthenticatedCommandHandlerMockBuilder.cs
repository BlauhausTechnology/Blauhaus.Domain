using System;
using System.Linq.Expressions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base
{

    public class AuthenticatedCommandHandlerMockBuilder<TMock, TPayload, TCommand, TUser> 
        : AuthenticatedCommandHandlerMockBuilder<AuthenticatedCommandHandlerMockBuilder<TMock, TPayload, TCommand, TUser>, TMock, TPayload, TCommand, TUser>
        where TMock : class, IAuthenticatedCommandHandler<TPayload, TCommand, TUser>
        where TCommand : notnull
        where TUser : notnull
    {

    }


    public class AuthenticatedCommandHandlerMockBuilder<TBuilder, TMock, TPayload, TCommand, TUser> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IAuthenticatedCommandHandler<TPayload, TCommand, TUser> 
        where TBuilder : AuthenticatedCommandHandlerMockBuilder<TBuilder, TMock, TPayload, TCommand, TUser>
        where TCommand : notnull
        where TUser : notnull
    {
        public TBuilder Where_HandleAsync_returns(TPayload payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(Response.Success(payload));
            return (TBuilder)this;
        }
        public TBuilder Where_HandleAsync_returns(Func<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(()=> Response.Success(payload.Invoke()));
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_returns_result(Response<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(),  It.IsAny<TUser>()))
                .ReturnsAsync(payload);
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_fails(string error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(Response.Failure<TPayload>(Error.Create(error)));
            return (TBuilder)this;
        }
        
        public TBuilder Where_HandleAsync_fails(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return (TBuilder)this;
        }
        public Error Where_HandleAsync_fails()
        {
            var error = Error.Create(Guid.NewGuid().ToString());
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return error;
        }


        public TBuilder Where_HandleAsync_returns_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<TUser>()))
                .ThrowsAsync(exception);
            return (TBuilder)this;
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