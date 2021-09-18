using System;
using System.Collections.Generic;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Errors;
using Blauhaus.Responses;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common.CommandHandlers._Base
{
    public class HandlerMockBuilder<TMock, TPayload>
        : HandlerMockBuilder<HandlerMockBuilder<TMock, TPayload>, TMock, TPayload>
        where TMock : class, IHandler<TPayload>
    {
    }

    public class HandlerMockBuilder<TBuilder, TMock, TPayload> : BaseMockBuilder<TBuilder, TMock>
        where TMock : class, IHandler<TPayload> 
        where TBuilder : HandlerMockBuilder<TBuilder, TMock, TPayload>
    {
         

        public TBuilder Where_HandleAsync_succeeds(TPayload payload)
        {
            Mock.Setup(x => x.HandleAsync())
                .ReturnsAsync(Response.Success(payload));
            return (TBuilder) this;
        }
        public TBuilder Where_HandleAsync_succeeds(Func<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync())
                .ReturnsAsync(()=> Response.Success(payload.Invoke()));
            return (TBuilder) this;
        }

        public TBuilder Where_HandleAsync_returns_sequence(List<TPayload> payloads)
        {
            var queue = new Queue<Response<TPayload>>();
            foreach (var payload in payloads)
            {
                queue.Enqueue(Response.Success(payload));
            }

            Mock.Setup(x => x.HandleAsync())
                .ReturnsAsync(queue.Dequeue);
            return (TBuilder) this;
        }

        public TBuilder Where_HandleAsync_returns_result(Response<TPayload> payload)
        {
            Mock.Setup(x => x.HandleAsync())
                .ReturnsAsync(payload);
            return (TBuilder) this;
        }
         
        
        public TBuilder Where_HandleAsync_fails(Error error)
        {
            Mock.Setup(x => x.HandleAsync())
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return (TBuilder) this;
        }
        public TBuilder Where_HandleAsync_fails()
        {
            var error = Error.Create(Guid.NewGuid().ToString());
            Mock.Setup(x => x.HandleAsync())
                .ReturnsAsync(Response.Failure<TPayload>(error));
            return (TBuilder) this;
        }


        public TBuilder Where_HandleAsync_throws(Exception exception)
        {
            Mock.Setup(x => x.HandleAsync())
                .ThrowsAsync(exception);
            return (TBuilder) this;
        }
         

        public void Verify_HandleAsync_called_Times(int times)
        {
            Mock.Verify(x => x.HandleAsync(), Times.Exactly(times));
        } 
         
         
    }
}