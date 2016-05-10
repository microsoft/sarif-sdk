// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Core
{
    internal class PropertyBagHolderTestClass : PropertyBagHolder
    {
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }
    }

    [TestClass]
    public class PropertyBagConverterTests
    {
        private string _input;
        private PropertyBagHolderTestClass _inputObject;
        private string _output;
        private PropertyBagHolderTestClass _roundTrippedObject;

        [TestMethod]
        public void PropertyBagConverter_RoundTripsStringValuedProperty()
        {
            _input = "{\"properties\":{\"s\":\"abc\"}}";

            PerformRoundTrip();

            _inputObject.GetProperty("s").Should().Be("abc");
            _roundTrippedObject.GetProperty("s").Should().Be("abc");
        }

        [TestMethod]
        public void PropertyBagConverter_RoundTripsIntegerValuedProperty()
        {
            _input = "{\"properties\":{\"n\":42}}";

            PerformRoundTrip();

            _inputObject.GetProperty<long>("n").Should().Be(42L);
            _roundTrippedObject.GetProperty<long>("n").Should().Be(42L);
        }

        [TestMethod]
        public void PropertyBagConverter_RoundTripsBooleanValuedProperty()
        {
            _input = "{\"properties\":{\"f\":true}}";

            PerformRoundTrip();

            _inputObject.GetProperty<bool>("f").Should().Be(true);
            _roundTrippedObject.GetProperty<bool>("f").Should().Be(true);
        }

        [TestMethod]
        public void PropertyBagConverter_RoundTripsFloatValuedProperty()
        {
            _input = "{\"properties\":{\"f\":3.14}}";

            PerformRoundTrip();

            _inputObject.GetProperty<double>("f").Should().Be(3.14);
            _roundTrippedObject.GetProperty<double>("f").Should().Be(3.14);
        }

        [TestMethod]
        public void PropertyBagConverter_RoundTripsArrayValuedProperty()
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

        public class ObjectClass
        {
            public int One { get; set; }
            public int Two { get; set; }
        }

        [TestMethod]
        public void PropertyBagConverter_RoundTripsObjectValuedProperty()
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
            _inputObject = JsonConvert.DeserializeObject<PropertyBagHolderTestClass>(_input);
            _output = JsonConvert.SerializeObject(_inputObject);
            _roundTrippedObject = JsonConvert.DeserializeObject<PropertyBagHolderTestClass>(_output);

            _output.Should().Be(_input);
        }
    }
}
