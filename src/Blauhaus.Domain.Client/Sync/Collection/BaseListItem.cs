using System;
using Blauhaus.Common.Utils.NotifyPropertyChanged;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Client.Sync.Collection
{
    public abstract class BaseListItem<TModel> : BaseBindableObject, IListItem<TModel> where TModel : IClientEntity
    { 

        public Guid Id { get; set; }
        public long ModifiedAtTicks { get; set; }
        public EntityState EntityState { get; private set; } = EntityState.Active;

        public virtual bool UpdateFromModel(TModel model)
        {
            Id = model.Id;
            ModifiedAtTicks = model.ModifiedAtTicks;
            EntityState = model.EntityState;

            return Update(model);
        }

        protected abstract bool Update(TModel model);

    }
}