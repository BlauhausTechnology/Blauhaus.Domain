using System;
using System.ComponentModel;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public interface IListItem<TModel> : IClientEntity<Guid>, INotifyPropertyChanged
        where TModel : IClientEntity<Guid>
    {
        bool UpdateFromModel(TModel model);
    }
}