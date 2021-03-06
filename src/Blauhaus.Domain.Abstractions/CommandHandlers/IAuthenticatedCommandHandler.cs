﻿using System.Threading.Tasks;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Abstractions.CommandHandlers
{
    public interface IAuthenticatedCommandHandler<TPayload, in TCommand, in TUser> 
        where TCommand : notnull
        where TUser : notnull
    {
        Task<Response<TPayload>> HandleAsync(TCommand command, TUser user);
    }


}