using System;
using System.Threading.Tasks;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.Utils.Disposables;
using Blauhaus.Domain.Abstractions.Actors;

namespace Blauhaus.Domain.Server.Actors
{
    public abstract class BaseServerModelActor<TModel, TId> : BasePublisher, IModelActor<TModel, TId> 
        where TId : IEquatable<TId> 
        where TModel : IHasId<TId>
    {

        private TId? _id;
        public TId Id
        {
            get
            {
                if (_id == null)
                    throw new InvalidOperationException("Actor has not been initialized with an Id");
                return _id;
            }
        }

        protected TModel? Model;

        public Task InitializeAsync(TId id)
        {
            _id = id;
            return OnInitializedAsync(id);
        }

        protected virtual Task OnInitializedAsync(TId id)
        {
            return Task.CompletedTask;
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

        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
        
        protected abstract Task<TModel> LoadModelAsync();
    }
}