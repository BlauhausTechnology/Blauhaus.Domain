﻿using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    
    public interface IVoidCommandHandler<TCommand> 
        where TCommand : notnull
    {
        Task<Response> HandleAsync(TCommand command, CancellationToken token);
    }
}