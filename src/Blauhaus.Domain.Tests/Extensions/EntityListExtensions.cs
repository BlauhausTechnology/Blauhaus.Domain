using System.Collections.Generic;
using Blauhaus.Domain.Abstractions.Entities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Blauhaus.Domain.Tests.Extensions
{
    public static class EntityListExtensions
    {
        public static bool VerifyEntities<TEntity>(this List<TEntity> expected, List<TEntity> actual) where TEntity : IServerEntity
        {
            ClassicAssert.AreEqual(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                ClassicAssert.AreEqual(expected[i].Id, actual[i].Id);
                ClassicAssert.AreEqual(expected[i].EntityState, actual[i].EntityState);
                ClassicAssert.AreEqual(expected[i].ModifiedAt, actual[i].ModifiedAt);
            }

            return true;
        }

    }
}