using System;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Client.Sqlite.DtoCaches;
using Blauhaus.Domain.Client.Sqlite.Entities;
using Blauhaus.Domain.Tests.TestObjects.Common;
using Newtonsoft.Json;
using SQLite;

namespace Blauhaus.Domain.Tests.TestObjects.Client
{
    public class MySyncedDtoEntity : SyncClientEntity<Guid>
    {

        public MySyncedDtoEntity()
        {
            
        }

        //for tests
        public MySyncedDtoEntity(MyDto dto)
        {
            Id = dto.Id;
            ModifiedAtTicks = dto.ModifiedAtTicks;
            EntityState = dto.EntityState;
            SerializedDto = JsonConvert.SerializeObject(dto);
            Name = dto.Name;
            SyncState = SyncState.InSync;
        }

        [Indexed]
        public string Name { get; set; } = null!;

        [Indexed]
        public Guid CategoryId { get; set; }
         
    }
}