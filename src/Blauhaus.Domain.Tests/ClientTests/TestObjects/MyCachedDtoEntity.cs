using System;
using Blauhaus.Domain.Client.Sqlite.DtoCaches;
using SQLite;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class MyCachedDtoEntity : CachedDtoEntity<MyCachedDtoEntity, MyDto, Guid>
    {
        
        [Indexed]
        public string Name { get; set; }

        protected override void PopulateAdditionalProperties(MyDto dto)
        {
            Name = dto.Name;
        }
    }
}