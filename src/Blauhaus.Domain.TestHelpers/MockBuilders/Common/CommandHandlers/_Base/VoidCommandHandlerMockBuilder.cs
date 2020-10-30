using System;
using System.Linq.Expressions;
using System.Threading;
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
    {
    }


    public class VoidCommandHandlerMockBuilder<TBuilder, TMock, TCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IVoidCommandHandler<TCommand> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
    { 
        public TBuilder Where_HandleAsync_returns_result(Response result)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            return this as TBuilder;
        }

        public TBuilder Where_HandleAsync_returns_fail(string error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.Failure(Error.Create(error)));
            return this as TBuilder;
        }
        
        public TBuilder Where_HandleAsync_returns_fail(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.Failure(error));
            return this as TBuilder;
        }


        public TBuilder Where_HandleAsync_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
            return this as TBuilder;
        }

        public void Verify_HandleAsync_called_With(Expression<Func<TCommand, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.Is<TCommand>(predicate), It.IsAny<CancellationToken>()));
        }

        public void Verify_HandleAsync_NOT_called_With(Expression<Func<TCommand, bool>> predicate)
        {
            Mock.Verify(x => x.HandleAsync(It.Is<TCommand>(predicate), It.IsAny<CancellationToken>()), Times.Never);
        }

        public void Verify_HandleAsync_NOT_called()
        {
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}