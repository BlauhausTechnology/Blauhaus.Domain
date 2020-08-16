using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Utils.NotifyPropertyChanged;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.Domain.Abstractions.CommandHandlers.Sync;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Errors.Handler;
using Blauhaus.Ioc.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public class SyncCollection<TModel, TListItem, TSyncCommand> : BaseBindableObject, ISyncCollection<TModel, TListItem, TSyncCommand>
        where TModel : class, IClientEntity
        where TListItem : class, IListItem<TModel>
        where TSyncCommand : SyncCommand, new()
    {
        private readonly IErrorHandler _errorHandler;
        private readonly IAnalyticsService _analyticsService;
        private readonly IConnectivityService _connectivityService;
        private readonly ISyncClient<TModel, TSyncCommand> _syncClient;
        private readonly IServiceLocator _serviceLocator;
        private IDisposable? _syncClientConnection;

        public SyncCollection(
            IErrorHandler errorHandler, 
            IAnalyticsService analyticsService,
            IConnectivityService connectivityService,
            ISyncClient<TModel, TSyncCommand> syncClient,
            IServiceLocator serviceLocator)
        {
            _errorHandler = errorHandler;
            _analyticsService = analyticsService;
            _connectivityService = connectivityService;
            _syncClient = syncClient;
            _serviceLocator = serviceLocator;


            SyncCommand = new TSyncCommand();
            SyncStatusHandler = new SyncStatusHandler();
            SyncRequirement = ClientSyncRequirement.Batch;
            ListItems = new ObservableCollection<TListItem>();

        }
        
        public ClientSyncRequirement SyncRequirement { get; set; }
        public ObservableCollection<TListItem> ListItems { get; }
        public TSyncCommand SyncCommand { get; }
        public ISyncStatusHandler SyncStatusHandler { get; }

        public void Initialize()
        {
            if (_syncClientConnection == null)
            {
                _syncClientConnection = _syncClient.Connect(SyncCommand, SyncRequirement, SyncStatusHandler)
                    .Subscribe(OnNext, OnError);
            }
            else
            {
                _syncClient.LoadNewFromClient();
            }
        }

        public void LoadNewFromServer()
        {
            _syncClient.LoadNewFromServer();
        }

        public void LoadNewFromClient()
        {
            _syncClient.LoadNewFromClient();
        }
         
        private void OnNext(TModel nextModel)
        {
            try
            {
                var existingElement = ListItems.FirstOrDefault(x => x.Id == nextModel.Id);
                if (existingElement == null)
                {
                    AddNewElement(nextModel);
                }
                else
                {
                    UpdateExistingElement(existingElement, nextModel);
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleExceptionAsync(this, e);
            }
        }

        private void UpdateExistingElement(TListItem existingElement, TModel model)
        {
            var isStillValid = existingElement.UpdateFromModel(model);
            if (!isStillValid)
            {
                _analyticsService.Trace(this, "Removing list item");
                ListItems.RemoveAt(ListItems.IndexOf(existingElement));
            }
            else
            {
                var currentIndex = ListItems.IndexOf(existingElement);
                var newIndex = 0;
                var numberOfItems = ListItems.Count;
                
                for (var i = 0; i < numberOfItems; i++)
                {
                    if (existingElement.ModifiedAtTicks > ListItems[i].ModifiedAtTicks)
                    {
                        newIndex = i;
                        break;
                    }
                }
                ListItems.Move(currentIndex, newIndex);
            }
        }

        private void AddNewElement(TModel model)
        {
            var newListItem = _serviceLocator.Resolve<TListItem>();

            var isValid = newListItem.UpdateFromModel(model);
            if (!isValid)
            {
                return;
            }

            var isAdded = false;
            var numberOfItems = ListItems.Count;
            for (var i = 0; i < numberOfItems; i++)
            {
                if (newListItem.ModifiedAtTicks > ListItems[i].ModifiedAtTicks)
                {
                    ListItems.Insert(i, newListItem);
                    isAdded = true;
                    break;
                }
            }
            if (!isAdded)
            {
                _analyticsService.Trace(this, "Adding list item");
                ListItems.Add(newListItem);
            }
        }

        private void OnError(Exception exception)
        {
            _errorHandler.HandleExceptionAsync(this, exception);
        }

    }
}