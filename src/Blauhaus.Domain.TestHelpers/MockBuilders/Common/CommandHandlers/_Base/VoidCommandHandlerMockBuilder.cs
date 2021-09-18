using System;
using System.Linq.Expressions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base
{
    public class VoidCommandHandlerMockBuilder<TMock, TCommand> 
        : VoidCommandHandlerMockBuilder<VoidCommandHandlerMockBuilder<TMock, TCommand>, TMock, TCommand>
        where TMock : class, IVoidCommandHandler<TCommand>
        where TCommand : notnull
    {
    }


    public class VoidCommandHandlerMockBuilder<TBuilder, TMock, TCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IVoidCommandHandler<TCommand> 
        where TBuilder : VoidCommandHandlerMockBuilder<TBuilder, TMock, TCommand>
        where TCommand : notnull
    { 
        public TBuilder Where_HandleAsync_returns_result(Response result)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(result);
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_fails(string error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Failure(Error.Create(error)));
            return (TBuilder)this;
        }
        
        public TBuilder Where_HandleAsync_fails(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Failure(error));
            return (TBuilder)this;
        }
        public Error Where_HandleAsync_fails()
        {
            var error = Error.Create(Guid.NewGuid().ToString());
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Failure(error));
            return error;
        }


        public TBuilder Where_HandleAsync_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ThrowsAsync(exception);
            return (TBuilder)this;
        }

        public void Verify_HandleAsync_called_With(Expression<Func<TCommand, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.Is<TCommand>(predicate)));
        }

        public void Verify_HandleAsync_NOT_called_With(Expression<Func<TCommand, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.Is<TCommand>(predicate)), Times.Never);
        }

        public void Verify_HandleAsync_NOT_called()
        {
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>()), Times.Never);
        }
    }
}