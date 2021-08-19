using Blauhaus.Common.Abstractions;
using Blauhaus.Domain.Abstractions.CommandHandlers;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.DtoCaches
{
    public class DtoCacheMockBuilder<TDto, TId> : BaseDtoCacheMockBuilder<DtoCacheMockBuilder<TDto, TId>, IDtoCache<TDto, TId>,TDto, TId> 
        where TDto : class, IHasId<TId>
    {

    }
}