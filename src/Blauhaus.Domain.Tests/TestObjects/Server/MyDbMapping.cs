﻿using System;
using Blauhaus.Domain.Server.EFCore.DbMappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blauhaus.Domain.Tests.TestObjects.Server
{
    public class MyDbMapping : BaseSyncDbMapping<MySyncServerEntity, Guid>
    {
        public MyDbMapping(ModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override void ConfigureEntity(EntityTypeBuilder<MySyncServerEntity> entity)
        {
            
        }
    }
}