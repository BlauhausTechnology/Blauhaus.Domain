using System;
using System.Linq.Expressions;
using System.Threading;
using Blauhaus.Common.ValueObjects.Errors;
using Blauhaus.Domain.Common.CommandHandlers;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.CommandHandlers._Base
{
    public class BaseCommandHandlerMockBuilder<TBuilder, TMock, TPayload, TCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ICommandHandler<TPayload, TCommand> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
    {
        public TBuilder Where_HandleAsync_returns(TPayload payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(payload));
            return this as TBuilder;
        }

        public TBuilder Where_HandleAsync_returns_result(Result<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payload);
            return this as TBuilder;
        }

        public TBuilder Where_HandleAsync_returns_fails(string error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<TPayload>(error));
            return this as TBuilder;
        }
        
        public TBuilder Where_HandleAsync_returns_fails(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<TPayload>(error.ToString()));
            return this as TBuilder;
        }


        public TBuilder Where_HandleAsync_returns_throws(Exception exception)
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