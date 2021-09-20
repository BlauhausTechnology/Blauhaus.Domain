using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;

namespace Blauhaus.Domain.Abstractions.DtoCaches
{
     
    public interface ISyncDtoCache<TDto, in TId> : IDtoCache<TDto, TId>
        where TDto : class, IClientEntity<TId> where TId : IEquatable<TId>
    {
        Task<long> LoadLastModifiedTicksAsync(IKeyValueProvider? settingsProvider);

    }
}