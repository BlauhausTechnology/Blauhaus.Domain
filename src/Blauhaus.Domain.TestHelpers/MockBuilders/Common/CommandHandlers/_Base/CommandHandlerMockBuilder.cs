using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using CSharpFunctionalExtensions;
using Moq;
using Newtonsoft.Json;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base
{
    public class CommandHandlerMockBuilder<TMock, TPayload, TCommand> 
        : CommandHandlerMockBuilder<CommandHandlerMockBuilder<TMock, TPayload, TCommand>, TMock, TPayload, TCommand>
        where TMock : class, ICommandHandler<TPayload, TCommand>
    {
    }

    public class CommandHandlerMockBuilder<TBuilder, TMock, TPayload, TCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ICommandHandler<TPayload, TCommand> 
        where TBuilder : BaseMockBuilder<TBuilder, TMock>
    {
         
        private readonly List<TCommand> _serializedCommands = new List<TCommand>();

        public TBuilder Where_HandleAsync_returns(TPayload payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.Success(payload))
                .Callback((TCommand command, CancellationToken token) =>
                {
                    //we need to serialilze the values because SyncCommand changes state during execution
                    _serializedCommands.Add(JsonConvert.DeserializeObject<TCommand>(JsonConvert.SerializeObject(command)));
                }); ;
            return this as TBuilder;
        }

        public TBuilder Where_HandleAsync_returns(List<TPayload> payloads)
        {
            var queue = new Queue<Response<TPayload>>();
            foreach (var payload in payloads)
            {
                queue.Enqueue(Response.Success(payload));
            }
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queue.Dequeue)
                .Callback((TCommand command, CancellationToken token) =>
                {
                    //we need to serialilze the values because SyncCommand changes state during execution
                    _serializedCommands.Add(JsonConvert.DeserializeObject<TCommand>(JsonConvert.SerializeObject(command)));
                }); ;
            return this as TBuilder;
        }

        public TBuilder Where_HandleAsync_returns_result(Response<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payload);
            return this as TBuilder;
        }
         
        
        public TBuilder Where_HandleAsync_returns_fail(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return this as TBuilder;
        }


        public TBuilder Where_HandleAsync_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
            return this as TBuilder;
        }
        
        public void Verify_HandleAsync_called_in_sequence(int callIndex, Func<TCommand, bool> predicate)
        {
            if (_serializedCommands.Count < callIndex + 1)
            {
                throw new IndexOutOfRangeException();
            }
                
            var commandAtIndex = _serializedCommands[callIndex];
            if (predicate.Invoke(commandAtIndex) == false)
            {
                throw new Exception("Expcted command not called");
            }
        }

        public void Verify_HandleAsync_called_Times(int times)
        {
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(times));
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