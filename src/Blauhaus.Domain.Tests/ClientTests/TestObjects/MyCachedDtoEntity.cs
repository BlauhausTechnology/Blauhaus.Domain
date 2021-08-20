using System;
using Blauhaus.Domain.Client.Sqlite.DtoCaches;
using Newtonsoft.Json;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class MyCachedDtoEntity : CachedDtoEntity<Guid>
    {

        public MyCachedDtoEntity()
        {
            
        }

        //for tests
        public MyCachedDtoEntity(MyDto dto)
        {
            Id = dto.Id;
            ModifiedAtTicks = dto.ModifiedAtTicks;
            EntityState = dto.EntityState;
            SerializedDto = JsonConvert.SerializeObject(dto);
            Name = dto.Name;
        }

        [Indexed]
        public string Name { get; set; }
         
    }
}