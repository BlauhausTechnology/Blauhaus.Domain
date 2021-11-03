using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;

namespace Blauhaus.Domain.Abstractions.DtoHandlers
{
    public interface IDtoHandler<in TDto>
    {
        Task HandleAsync(TDto dto);
    }

}