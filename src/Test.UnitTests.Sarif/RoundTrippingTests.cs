// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Globalization;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class RoundTrippingTests
    {
        private static readonly ResourceExtractor s_extractor = new ResourceExtractor(typeof(RoundTrippingTests));

        private static string GetResourceContents(string resourceName)
            => s_extractor.GetResourceText($"RoundTripping.{resourceName}");

        // Class used to test serialization and deserialization of an arbitrary class.
        public class CustomObject
        {
            public DateTime DateTime { get; set; }
            public string String { get; set; }
        }

        private static DateTime ParseUtcDateTime(string dateTime)
        {
            // DateTimeStyles.AdjustToUniversal is necessary because otherwise DateTime.Parse
            // adjusts the time from UTC to local time (for example, in Redmond, it subtracts
            // 7 hours). We want the hours field to be exactly as specified in the test file.
            return DateTime.Parse(dateTime, provider: null, DateTimeStyles.AdjustToUniversal);
        }

        [Fact]
        public void SarifLog_PropertyBagProperties_CanBeRoundTripped()
        {
            string originalContents = GetResourceContents("RoundTripping.sarif");

            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(originalContents);

            PropertyBagHolder holder = log.Runs[0].Results[0];

            DateTime dateTimeProperty = holder.GetProperty<DateTime>("dateTime");
            dateTimeProperty.Should().Be(ParseUtcDateTime("2019-09-25T15:07:01Z"));

            string stringProperty = holder.GetProperty("string");
            stringProperty.Should().Be("Hello, string!");

            int intProperty = holder.GetProperty<int>("int");
            intProperty.Should().Be(42);

            long longProperty = holder.GetProperty<long>("long");
            longProperty.Should().Be(5000000000);

            // This integer-valued JSON property is too big for an Int32.
            Action action = () => holder.GetProperty<int>("long");
            action.Should().Throw<JsonReaderException>();

            double doubleProperty = holder.GetProperty<double>("double");
            doubleProperty.Should().Be(3.14159265);

            bool trueProperty = holder.GetProperty<bool>("true");
            trueProperty.Should().BeTrue();

            bool falseProperty = holder.GetProperty<bool>("false");
            falseProperty.Should().BeFalse();

            string nullProperty = holder.GetProperty("null");
            nullProperty.Should().BeNull();

            DateTime[] dateTimeArray = holder.GetProperty<DateTime[]>("dateTimeArray");
            dateTimeArray.Length.Should().Be(3);
            dateTimeArray[0].Should().Be(ParseUtcDateTime("2019-09-26T15:52:02Z"));
            dateTimeArray[1].Should().Be(ParseUtcDateTime("2019-09-26T15:54Z"));
            // Round-tripping works exactly with up to 7 decimal digits after the seconds. This
            // is because Newtonsoft.Json's default DateTime serialization places the first seven
            // decimal digits at the end of the Ticks count (for example, 637051101051234567).
            // If the next digit is less than 5, the comparison will work precisely no matter
            // how many additional digits there are; otherwise, the comparison will fail).
            dateTimeArray[2].Should().Be(ParseUtcDateTime("2019-09-26T15:55:05.12345673"));

            string[] stringArray = holder.GetProperty<string[]>("stringArray");
            stringArray.Length.Should().Be(2);
            stringArray[0].Should().Be("Thing One");
            stringArray[1].Should().Be("Thing Two");

            int[] intArray = holder.GetProperty<int[]>("intArray");
            intArray.Length.Should().Be(2);
            intArray[0].Should().Be(54);
            intArray[1].Should().Be(-54);

            bool[] boolArray = holder.GetProperty<bool[]>("boolArray");
            boolArray.Length.Should().Be(2);
            boolArray[0].Should().Be(true);
            boolArray[1].Should().Be(false);

            string[] nullArray = holder.GetProperty<string[]>("nullArray");
            nullArray.Length.Should().Be(2);
            nullArray[0].Should().BeNull();
            nullArray[1].Should().BeNull();

            CustomObject customObject = holder.GetProperty<CustomObject>("customObject");
            customObject.DateTime.Should().Be(ParseUtcDateTime("1776-07-04T12:00Z"));
            customObject.String.Should().Be("The 4th");

            string roundTrippedContents = JsonConvert.SerializeObject(log, Formatting.Indented);

            roundTrippedContents.Should().Be(originalContents);
        }
    }
}
