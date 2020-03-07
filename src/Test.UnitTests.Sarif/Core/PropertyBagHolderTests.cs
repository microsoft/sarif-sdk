// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    internal class TestClass : PropertyBagHolder
    {
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }
    }

    public class PropertyBagHolderTests
    {
        private const string PropertyName = "prop";

        [Fact]
        public void PropertyBagHolder_InitiallyHasNoProperties()
        {
            var testObject = new TestClass();

            testObject.PropertyNames.Count.Should().Be(0);
            testObject.ShouldNotContainProperty(PropertyName);
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_AddsSpecifiedProperty()
        {
            var inputObject = new TestClass();

            inputObject.SetProperty(PropertyName, "x");

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty(PropertyName).Should().Be("x");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_EscapesCharacters()
        {
            var inputObject = new TestClass();

            inputObject.SetProperty(PropertyName, @"\r""\t");

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty(PropertyName).Should().Be(@"\\r\""\\t");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_WorksWithNull()
        {
            var inputObject = new TestClass();

            inputObject.SetProperty<string>(PropertyName, null);

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty(PropertyName).Should().BeNull();
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_OverwritesExistingProperty()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            inputObject.SetProperty(PropertyName, 2);

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty<int>(PropertyName).Should().Be(2);
        }

        [Fact]
        public void PropertyBagHolder_GetProperty_ThrowsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            Action action = () => inputObject.GetProperty(PropertyName);

            action.Should().Throw<InvalidOperationException>().WithMessage($"*{PropertyName}*");
        }

        [Fact]
        public void PropertyBagHolder_GetPropertyOfT_ThrowsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            Action action = () => inputObject.GetProperty<int>(PropertyName);

            action.Should().Throw<InvalidOperationException>().WithMessage($"*{PropertyName}*");
        }

        [Fact]
        public void PropertyBagHolder_GetSerializedPropertyValue_ThrowsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            Action action = () => inputObject.GetSerializedPropertyValue(PropertyName);

            action.Should().Throw<InvalidOperationException>().WithMessage($"*{PropertyName}*");
        }

        [Fact]
        public void PropertyBagHolder_TryGetProperty_ReturnsTrueWhenPropertyExists()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            inputObject.TryGetProperty(PropertyName, out string value).Should().BeTrue();
            value.Should().Be("x");
        }

        [Fact]
        public void PropertyBagHolder_TryGetProperty_ReturnsFalseWhenPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            inputObject.TryGetProperty(PropertyName, out string value).Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void PropertyBagHolder_TryGetPropertyOfT_ReturnsTrueWhenPropertyExists()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, 42);

            inputObject.TryGetProperty<int>(PropertyName, out int value).Should().BeTrue();
            value.Should().Be(42);
        }

        [Fact]
        public void PropertyBagHolder_TryGetPropertyOfT_ReturnsFalseWhenPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            inputObject.TryGetProperty<int>(PropertyName, out int value).Should().BeFalse();
            value.Should().Be(0);
        }

        [Fact]
        public void PropertyBagHolder_TryGetSerializedPropertyValue_ReturnsTrueWhenPropertyExists()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, 42);

            inputObject.TryGetSerializedPropertyValue(PropertyName, out string value).Should().BeTrue();
            value.Should().Be("42");
        }

        [Fact]
        public void PropertyBagHolder_TryGetSerializedPropertyValue_ReturnsFalseWhenPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            inputObject.TryGetSerializedPropertyValue(PropertyName, out string value).Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void PropertyBagHolder_GetPropertyOfT_WorksForStringProperties()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            string value = inputObject.GetProperty<string>(PropertyName);
            value.Should().Be("x");
        }

        [Fact]
        public void PropertyBagHolder_RemoveProperty_RemovesExistingProperty()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            inputObject.RemoveProperty(PropertyName);

            inputObject.PropertyNames.Count.Should().Be(0);
            inputObject.ShouldNotContainProperty(PropertyName);
        }

        [Fact]
        public void PropertyBagHolder_RemoveProperty_SucceedsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();
            inputObject.ShouldNotContainProperty(PropertyName);

            inputObject.RemoveProperty(PropertyName);
            inputObject.ShouldNotContainProperty(PropertyName);
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsStringProperty()
        {
            "def".ShouldSerializeAs("\"def\"");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsIntegerProperty()
        {
            42.ShouldSerializeAs("42");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsLongProperty()
        {
            42L.ShouldSerializeAs("42");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsBooleanProperty()
        {
            true.ShouldSerializeAs("true");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsIntegerArrayProperty()
        {
            new int[] { 5, 12, 13 }.ShouldSerializeAs("[5,12,13]");
        }

        // This demonstrates how the "tags" property is handled.
        [Fact]
        public void PropertyBagHolder_SetProperty_SetsStringArrayProperty()
        {
            new string[] { "ab", "cde" }.ShouldSerializeAs("[\"ab\",\"cde\"]");
        }

        // These tests show that any enumerable serializes as an array.
        [Fact]
        public void PropertyBagHolder_SetProperty_SetsStringArrayPropertyFromList()
        {
            new List<string> { "ab", "cde" }.ShouldSerializeAs("[\"ab\",\"cde\"]");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsStringArrayPropertyFromHashSet()
        {
            new HashSet<string> { "ab", "cde" }.ShouldSerializeAs("[\"ab\",\"cde\"]");
        }

        public class TestObjectClass
        {
            public TestObjectClass(int n, string s)
            {
                N = n;
                S = s;
            }

            [JsonProperty("n")]
            public int N { get; }

            [JsonProperty("s")]
            public string S { get; }
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsObjectProperty()
        {
            new TestObjectClass(42, "abc").ShouldSerializeAs("{\"n\":42,\"s\":\"abc\"}");
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsGuidProperty()
        {
            const string GuidString = "{12345678-1234-1234-1234-1234567890ab}";

            // Serializing will strip the braces. We could change SetProperty to
            // special-case Guid and write it out with braces, but there's no
            // point.
            string expectedOutput = '"' + GuidString.Substring(1, GuidString.Length - 2) + '"';

            new Guid(GuidString).ShouldSerializeAs(expectedOutput);
        }

        [Fact]
        public void PropertyBagHolder_SetProperty_SetsDateTimeProperty()
        {
            // DateTime instances should not be converted to UTC when persisted to a property bag.
            // The reason is that we can't be  sure that the  user has an actual UTC time in hand,
            // or even the ability to convert the property date time to UTC.
            new DateTime(2016, 5, 11, 14, 28, 36, 123).ShouldSerializeAs("\"2016-05-11T14:28:36.123\"");
        }
    }

    internal static class ExtensionsForPropertyBagHolderTests
    {
        const string PropertyName = "prop";
        const string Input = "{\"properties\":{\"" + PropertyName + "\":12}}";

        internal static void ShouldSerializeAs<T>(this T value, string serializedValue)
        {
            string expectedOutput = "{\"properties\":{\"" + PropertyName + "\":" + serializedValue + "}}";
            TestClass inputObject = JsonConvert.DeserializeObject<TestClass>(Input);
            inputObject.GetProperty<long>(PropertyName).Should().Be(12);

            inputObject.SetProperty(PropertyName, value);

            string serializedObject = JsonConvert.SerializeObject(inputObject);
            serializedObject.Should().Be(expectedOutput);
        }

        internal static void ShouldContainProperty(this TestClass testObject, string propertyName)
        {
            testObject.ContainsProperty(propertyName).Should().BeTrue();
        }

        internal static void ShouldNotContainProperty(this TestClass testObject, string propertyName)
        {
            testObject.ContainsProperty(propertyName).Should().BeFalse();
        }

        private static bool ContainsProperty(this TestClass testObject, string propertyName)
        {
            return testObject.PropertyNames.Contains(propertyName);
        }
    }
}
