using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Common.Utils.NotifyPropertyChanged;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Common.CommandHandlers.Sync;
using Blauhaus.Domain.Common.Entities;
using Blauhaus.Errors.Handler;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public class SyncCollection<TModel, TListItem, TSyncCommand> : BaseBindableObject, ISyncCollection<TListItem, TSyncCommand>
        where TModel : class, IClientEntity
        where TListItem : ListItem, new()
        where TSyncCommand : SyncCommand, new()
    {
        private readonly IErrorHandler _errorHandler;
        private readonly IAnalyticsService _analyticsService;
        private readonly IConnectivityService _connectivityService;
        private readonly ISyncClient<TModel, TSyncCommand> _syncClient;
        private readonly IListItemUpdater<TModel, TListItem> _listItemUpdater;
        private IDisposable? _syncClientConnection;

        public SyncCollection(
            IErrorHandler errorHandler, 
            IAnalyticsService analyticsService,
            IConnectivityService connectivityService,
            ISyncClient<TModel, TSyncCommand> syncClient,
            IListItemUpdater<TModel, TListItem> listItemUpdater)
        {
            _errorHandler = errorHandler;
            _analyticsService = analyticsService;
            _connectivityService = connectivityService;
            _syncClient = syncClient;
            _listItemUpdater = listItemUpdater;

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

        public void Refresh()
        {
            _syncClient.LoadNewFromServer();
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
            existingElement = _listItemUpdater.Update(model, existingElement);
            existingElement.ModifiedAtTicks = model.ModifiedAtTicks;
                
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

        //todo add IsVisible property and remove / do not add if false;
        private void AddNewElement(TModel model)
        {
            var newListItem = _listItemUpdater.Update(model, new TListItem
            {
                Id = model.Id,
                ModifiedAtTicks = model.ModifiedAtTicks
            });

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
                ListItems.Add(newListItem);
            }
        }

        private void OnError(Exception exception)
        {
            _errorHandler.HandleExceptionAsync(this, exception);
        }

    }
}