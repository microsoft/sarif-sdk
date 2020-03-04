// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    internal class ConverterTestClass : PropertyBagHolder
    {
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }
    }

    public class PropertyBagConverterTests
    {
        private string _input;
        private ConverterTestClass _inputObject;
        private string _output;
        private ConverterTestClass _roundTrippedObject;

        [Fact]
        public void PropertyBagConverter_RoundTripsStringProperty()
        {
            _input = "{\"properties\":{\"s\":\"abc\"}}";

            PerformRoundTrip();

            _inputObject.GetProperty("s").Should().Be("abc");
            _roundTrippedObject.GetProperty("s").Should().Be("abc");
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsIntegerProperty()
        {
            _input = "{\"properties\":{\"n\":42}}";

            PerformRoundTrip();

            _inputObject.GetProperty<long>("n").Should().Be(42L);
            _roundTrippedObject.GetProperty<long>("n").Should().Be(42L);
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsBooleanProperty()
        {
            _input = "{\"properties\":{\"f\":true}}";

            PerformRoundTrip();

            _inputObject.GetProperty<bool>("f").Should().Be(true);
            _roundTrippedObject.GetProperty<bool>("f").Should().Be(true);
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsFloatProperty()
        {
            _input = "{\"properties\":{\"f\":3.14}}";

            PerformRoundTrip();

            _inputObject.GetProperty<double>("f").Should().Be(3.14);
            _roundTrippedObject.GetProperty<double>("f").Should().Be(3.14);
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsArrayProperty()
        {
            _input =
@"{""properties"":{""a"":[
  1,
  2
]}}";

            PerformRoundTrip();

            long[] array = _inputObject.GetProperty<long[]>("a");
            array.SequenceEqual(new[] { 1L, 2L });

            array = _roundTrippedObject.GetProperty<long[]>("a");
            array.SequenceEqual(new[] { 1L, 2L });
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsGuidProperty()
        {
            _input = "{\"properties\":{\"g\":\"{12345678-90ab-cdef-1234-567890abcdef}\"}}";

            PerformRoundTrip();

            Guid expectedGuid = new Guid("{12345678-90ab-cdef-1234-567890abcdef}");

            _inputObject.GetProperty<Guid>("g").Should().Be(expectedGuid);
            _roundTrippedObject.GetProperty<Guid>("g").Should().Be(expectedGuid);
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsGuidPropertyWithoutBraces()
        {
            _input = "{\"properties\":{\"g\":\"12345678-90ab-cdef-1234-567890abcdef\"}}";

            PerformRoundTrip();

            Guid expectedGuid = new Guid("{12345678-90ab-cdef-1234-567890abcdef}");

            _inputObject.GetProperty<Guid>("g").Should().Be(expectedGuid);
            _roundTrippedObject.GetProperty<Guid>("g").Should().Be(expectedGuid);
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1581")]
        public void PropertyBagConverter_RoundTripsNullValueForReferenceType()
        {
            _input = "{\"properties\":{\"a\":null}}";

            PerformRoundTrip();

            _inputObject.GetProperty<string>("a").Should().BeNull();
            _roundTrippedObject.GetProperty<string>("a").Should().BeNull();
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1581")]
        public void PropertyBagConverter_RoundTripsNullValueForValueType()
        {
            _input = "{\"properties\":{\"a\":null}}";

            PerformRoundTrip();

            Action action = () => _inputObject.GetProperty<int>("a");
            action.Should().Throw<InvalidOperationException>();

            action = () => _roundTrippedObject.GetProperty<int>("a");
            action.Should().Throw<InvalidOperationException>();
        }

        public class ObjectClass
        {
            public int One { get; set; }
            public int Two { get; set; }
        }

        [Fact]
        public void PropertyBagConverter_RoundTripsObjectProperty()
        {
            _input =
@"{""properties"":{""o"":{
  ""one"": 1,
  ""two"": 2
}}}";

            PerformRoundTrip();

            ObjectClass obj = _inputObject.GetProperty<ObjectClass>("o");
            obj.One.Should().Be(1);
            obj.Two.Should().Be(2);

            obj = _roundTrippedObject.GetProperty<ObjectClass>("o");
            obj.One.Should().Be(1);
            obj.Two.Should().Be(2);
        }

        private void PerformRoundTrip()
        {
            _inputObject = JsonConvert.DeserializeObject<ConverterTestClass>(_input);
            _output = JsonConvert.SerializeObject(_inputObject);
            _roundTrippedObject = JsonConvert.DeserializeObject<ConverterTestClass>(_output);

            _output.Should().BeCrossPlatformEquivalentStrings(_input);
        }
    }
}
