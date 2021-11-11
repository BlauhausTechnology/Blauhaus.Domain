using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public abstract class BaseServerModelActor<TModel, TId> : BasePublisher, IModelActor<TModel, TId> 
        where TId : IEquatable<TId> 
        where TModel : IHasId<TId>
    {
        
        private TId? _id;
        protected TModel? Model;
        
        protected readonly IAnalyticsService AnalyticsService;

        protected BaseServerModelActor(IAnalyticsService analyticsService)
        {
            AnalyticsService = analyticsService;
        }

        public TId Id
        {
            get
            {
                if (_id == null)
                    throw new InvalidOperationException("Actor has not been initialized with an Id");
                return _id;
            }
        }


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

        
        protected async Task<Response> TryExecuteAsync(Func<Task<Response>> func, string operationName, Dictionary<string, object> properties)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{operationName} executed by {GetType().Name}", LogSeverity.Verbose, properties))
            {
                try
                {
                    return await func.Invoke();
                }
                catch (Exception e)
                {
                    if (e.IsErrorException())
                    {
                        return AnalyticsService.TraceErrorResponse(this, e.ToError());
                    }
                    AnalyticsService.LogException(this, e, properties);
                    return Response.Failure(Error.Unexpected($"{operationName} failed to complete"));
                }
            }
        }
        
        protected async Task<Response<T>> TryExecuteAsync<T>(Func<Task<Response<T>>> func, string operationName, Dictionary<string, object> properties)
        {
            using (var _ = AnalyticsService.StartTrace(this, $"{operationName} executed by {GetType().Name}", LogSeverity.Verbose, properties))
            {
                try
                {
                    return await func.Invoke();
                }
                catch (Exception e)
                {
                    if (e.IsErrorException())
                    {
                        return AnalyticsService.TraceErrorResponse<T>(this, e.ToError());
                    }
                    AnalyticsService.LogException(this, e, properties);
                    return Response.Failure<T>(Error.Unexpected($"{operationName} failed to complete"));
                }
            }
        }

       

        protected Response TraceError(Error error)
        {
            return AnalyticsService.TraceErrorResponse(this, error);
        }
        
        protected Response<T> TraceError<T>(Error error)
        {
            return AnalyticsService.TraceErrorResponse<T>(this, error);
        }
    }
}