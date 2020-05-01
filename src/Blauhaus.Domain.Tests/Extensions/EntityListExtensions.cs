using System.Collections.Generic;
using Blauhaus.Domain.Client.Entities;
using Blauhaus.Domain.Server.Entities;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.Extensions
{
    public static class EntityListExtensions
    {
        public static bool VerifyEntities<TEntity>(this List<TEntity> expected, List<TEntity> actual) where TEntity : IServerEntity
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id);
                Assert.AreEqual(expected[i].EntityState, actual[i].EntityState);
                Assert.AreEqual(expected[i].ModifiedAt, actual[i].ModifiedAt);
            }

            return true;
        }

    }
}