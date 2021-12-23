using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;
using Newtonsoft.Json;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base
{
    public class CommandHandlerMockBuilder<TMock, TPayload, TCommand> 
        : CommandHandlerMockBuilder<CommandHandlerMockBuilder<TMock, TPayload, TCommand>, TMock, TPayload, TCommand>
        where TMock : class, ICommandHandler<TPayload, TCommand>        
        where TCommand : notnull
    {
    }

    public class CommandHandlerMockBuilder<TBuilder, TMock, TPayload, TCommand> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, ICommandHandler<TPayload, TCommand> 
        where TBuilder : CommandHandlerMockBuilder<TBuilder, TMock, TPayload, TCommand>  
        where TCommand : notnull
    {
         
        private readonly List<TCommand> _serializedCommands = new List<TCommand>();

        public TBuilder Where_HandleAsync_returns(TPayload payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Success(payload))
                .Callback((TCommand command) =>
                {
                    //we need to serialilze the values because SyncCommand changes state during execution
                    _serializedCommands.Add(JsonConvert.DeserializeObject<TCommand>(JsonConvert.SerializeObject(command))!);
                }); ;
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_returns_sequence(List<TPayload> payloads)
        {
            var queue = new Queue<Response<TPayload>>();
            foreach (var payload in payloads)
            {
                queue.Enqueue(Response.Success(payload));
            }
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(queue.Dequeue)
                .Callback((TCommand command) =>
                {
                    //we need to serialilze the values because SyncCommand changes state during execution
                    _serializedCommands.Add(JsonConvert.DeserializeObject<TCommand>(JsonConvert.SerializeObject(command))!);
                }); ;
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_returns_sequence(List<Response<TPayload>> payloads)
        {
            var queue = new Queue<Response<TPayload>>();
            foreach (var payload in payloads)
            {
                queue.Enqueue(payload);
            }
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(queue.Dequeue)
                .Callback((TCommand command) =>
                {
                    //we need to serialilze the values because SyncCommand changes state during execution
                    _serializedCommands.Add(JsonConvert.DeserializeObject<TCommand>(JsonConvert.SerializeObject(command))!);
                });
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_returns_result(Response<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(payload);
            return (TBuilder)this;
        }
         
        public TBuilder Where_HandleAsync_succeds(TPayload payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Success(payload));
            return (TBuilder)this;
        }

        public TBuilder Where_HandleAsync_succeds(Func<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(()=> Response.Success(payload.Invoke()));
            return (TBuilder)this;
        }
        
        public TBuilder Where_HandleAsync_fails(Error error)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return (TBuilder)this;
        }
        public TBuilder Where_HandleAsync_fails()
        {
            var error = Error.Create(Guid.NewGuid().ToString());
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return (TBuilder)this;
        }


        public TBuilder Where_HandleAsync_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync(It.IsAny<TCommand>()))
                .ThrowsAsync(exception);
            return (TBuilder)this;
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
            Mock.Verify(x => x.HandleAsync(It.IsAny<TCommand>()), Times.Exactly(times));
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