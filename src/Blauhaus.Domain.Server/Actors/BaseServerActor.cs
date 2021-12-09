using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Abstractions;
using Blauhaus.Common.Utils.Disposables;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Extensions;
using Blauhaus.Errors.Extensions;
using Blauhaus.Errors;
using Blauhaus.Responses;
using System.Collections.Generic;

namespace Blauhaus.Domain.Server.Actors
{
    public abstract class BaseServerActor<TId> : BasePublisher, IHasId<TId>, IAsyncInitializable<TId>, IAsyncDisposable
    {
        
        protected readonly IAnalyticsService AnalyticsService;
        
        private TId? _id;

        protected BaseServerActor(IAnalyticsService analyticsService)
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

        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        
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