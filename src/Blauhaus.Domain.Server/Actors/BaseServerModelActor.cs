using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Domain.Abstractions.Actors;
using Blauhaus.Errors.Extensions;
using Blauhaus.Errors;
using Blauhaus.Responses;

namespace Blauhaus.Domain.Server.Actors
{
    public abstract class BaseServerModelActor<TModel, TId> : BaseServerActor<TId>, IModelActor<TModel, TId> 
        where TId : IEquatable<TId> 
        where TModel : IHasId<TId>
    {
        
        protected TModel? Model;
        
        protected BaseServerModelActor(
            IAnalyticsLogger logger) 
            : base(logger)
        {
        }
        public Task<IDisposable> SubscribeAsync(Func<TModel, Task> handler, Func<TModel, bool>? filter = null)
        {
            return Task.FromResult(AddSubscriber(handler, filter));
        }

        public async Task ReloadAsync()
        {
            Model = await LoadModelAsync();
            await UpdateSubscribersAsync(Model);
        }

        public async Task<TModel> GetModelAsync()
        {
            return Model ??= await LoadModelAsync();
        }

        protected abstract Task<TModel> LoadModelAsync();

        
    }
}