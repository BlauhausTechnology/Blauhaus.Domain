using System;
using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Entities
{
    public abstract class BaseClientEntity : IClientEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        
        [Indexed]
        public EntityState EntityState { get; set;}
        
        [Indexed]
        public long ModifiedAtTicks { get; set;}
         
    }
}