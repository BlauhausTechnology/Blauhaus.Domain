using System;
using Blauhaus.Domain.Abstractions.Extensions;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.CommonTests.ExtensionsTests.LongExtensionsTests
{
    public class ToDateTimeOffsetTests
    {
        [Test]
        public void SHOULD_convert_ticks_to_DateTimeOffset()
        {
            //Arrange
            var dateTimeOffset = DateTimeOffset.UtcNow;

            //Act
            var result = dateTimeOffset.Ticks.ToDateTimeOffset();

            //Assert
            Assert.That(result, Is.EqualTo(dateTimeOffset)); 
        }

        [Test]
        public void WHEN_given_nullable_SHOULD_convert_ticks_to_UTC_DateTime()
        {
            //Arrange
            var dateTimeOffset = (DateTimeOffset?) DateTimeOffset.UtcNow;

            //Act
            var result = ((long?)dateTimeOffset.Value.Ticks).ToDateTimeOffset();

            //Assert
            Assert.That(result, Is.EqualTo(dateTimeOffset)); 
        }

        [Test]
        public void WHEN_given_null_SHOULD_return_null()
        {
            //Arrange
            long? dateTimeOffset = null;

            //Act
            var result = (dateTimeOffset).ToDateTimeOffset();

            //Assert
            Assert.That(result, Is.Null);
        }
    }
}