using System;
using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Models
{
    public abstract class BaseClientModel<TRootEntity> : IClientEntity where TRootEntity : IClientEntity
    {
        protected TRootEntity RootEntity;

        protected BaseClientModel(TRootEntity entity)
        {
            Id = entity.Id;
            EntityState = entity.EntityState;
            ModifiedAtTicks = entity.ModifiedAtTicks;
            RootEntity = entity;
        }

        public Guid Id { get; protected set; }
        public EntityState EntityState { get; protected set;}
        public long ModifiedAtTicks { get; protected set;}

    }
}