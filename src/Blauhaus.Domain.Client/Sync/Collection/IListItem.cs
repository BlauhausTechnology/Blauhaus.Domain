﻿using System.ComponentModel;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public interface IListItem<TModel> : IClientEntity, INotifyPropertyChanged
        where TModel : IClientEntity
    {
        bool UpdateFromModel(TModel model);
    }
}