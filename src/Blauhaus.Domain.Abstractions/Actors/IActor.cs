using Blauhaus.Common.Abstractions;
using System;

namespace Blauhaus.Domain.Abstractions.Actors
{
    public interface IActor<TId> : IAsyncDisposable, IAsyncInitializable<TId>, IAsyncReloadable, IHasId<TId>
    {
        
    }
}