﻿using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    
    public interface IVoidCommandHandler<TCommand> 
        where TCommand : notnull
    {
        Task<Result> HandleAsync(TCommand command, CancellationToken token);
    }
}