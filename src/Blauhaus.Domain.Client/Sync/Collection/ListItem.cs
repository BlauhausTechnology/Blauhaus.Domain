using System;
using Blauhaus.Common.Utils.NotifyPropertyChanged;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public abstract class ListItem<TModel> : BaseBindableObject, IListItem<TModel> where TModel : IClientEntity
    { 

        public Guid Id { get; set; }
        public long ModifiedAtTicks { get; set; }
        public EntityState EntityState { get; private set; } = EntityState.Active;
        public bool IsVisible { get; set; } //todo

        public virtual void UpdateFromModel(TModel model)
        {
            Id = model.Id;
            ModifiedAtTicks = model.ModifiedAtTicks;
            EntityState = model.EntityState;

            Update(model);
        }

        protected abstract void Update(TModel model);

    }
}