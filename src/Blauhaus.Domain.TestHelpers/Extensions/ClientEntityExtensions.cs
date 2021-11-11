using System;
using Blauhaus.Domain.Abstractions.Entities;
using NUnit.Framework;

namespace Blauhaus.Domain.TestHelpers.Extensions
{
    public static class ClientEntityExtensions 
    {
        public static void VerifyAgainstServerEntity(this IClientEntity<Guid> clientEntity, IServerEntity serverEntity)
        {
            Assert.That(clientEntity.Id, Is.EqualTo(serverEntity.Id));
            Assert.That(clientEntity.EntityState, Is.EqualTo(serverEntity.EntityState));
            Assert.That(clientEntity.ModifiedAtTicks, Is.EqualTo(serverEntity.ModifiedAt.Ticks));
        }
    }
}