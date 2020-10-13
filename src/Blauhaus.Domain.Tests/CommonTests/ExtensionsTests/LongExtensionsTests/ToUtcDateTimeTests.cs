using System;
using Blauhaus.Domain.Abstractions.Extensions;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.CommonTests.ExtensionsTests.LongExtensionsTests
{
    public class ToUtcDateTimeTests
    {
        [Test]
        public void SHOULD_convert_ticks_to_UTC_DateTime()
        {
            //Arrange
            var utcDateTime = DateTime.UtcNow;

            //Act
            var result = utcDateTime.Ticks.ToUtcDateTime();

            //Assert
            Assert.That(result, Is.EqualTo(utcDateTime));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void WHEN_given_nullable_SHOULD_convert_ticks_to_UTC_DateTime()
        {
            //Arrange
            var utcDateTime = (DateTime?)DateTime.UtcNow;

            //Act
            var result = ((long?)utcDateTime.Value.Ticks).ToUtcDateTime();

            //Assert
            Assert.That(result, Is.EqualTo(utcDateTime));
            Assert.That(result.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void WHEN_given_null_SHOULD_return_null()
        {
            //Arrange
            long? utcDateTime = null;

            //Act
            var result = (utcDateTime).ToUtcDateTime();

            //Assert
            Assert.That(result, Is.Null);
        }
    }
}