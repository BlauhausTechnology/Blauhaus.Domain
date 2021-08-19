using System;
using Blauhaus.Domain.Abstractions.Entities;
using SQLite;

namespace Blauhaus.Domain.Client.Sqlite.Entities
{

    public class ClientEntity : ClientEntity<Guid>, IClientEntity
    { 
         
    }


    public class ClientEntity<TId> : IClientEntity<TId>
    {
        [PrimaryKey]
        public TId Id { get; set; }
        
        [Indexed]
        public EntityState EntityState { get; set;}
        
        [Indexed]
        public long ModifiedAtTicks { get; set;}
         
    }
}