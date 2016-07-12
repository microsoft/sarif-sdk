// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Core
{
    internal class TestClass : PropertyBagHolder
    {
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }
    }

    [TestClass]
    public class PropertyBagHolderTests
    {
        private const string PropertyName = "prop";

        [TestMethod]
        public void PropertyBagHolder_InitiallyHasNoProperties()
        {
            var testObject = new TestClass();

            testObject.PropertyNames.Count.Should().Be(0);
            testObject.ShouldNotContainProperty(PropertyName);
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_AddsSpecifiedProperty()
        {
            var inputObject = new TestClass();

            inputObject.SetProperty(PropertyName, "x");

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty(PropertyName).Should().Be("x");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_EscapesCharacters()
        {
            var inputObject = new TestClass();

            inputObject.SetProperty(PropertyName, @"\r""\t");

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty(PropertyName).Should().Be(@"\\r\""\\t");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_WorksWithNull()
        {
            var inputObject = new TestClass();

            inputObject.SetProperty<string>(PropertyName, null);

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty(PropertyName).Should().BeNull();
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_OverwritesExistingProperty()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            inputObject.SetProperty(PropertyName, 2);

            inputObject.PropertyNames.Count.Should().Be(1);
            inputObject.ShouldContainProperty(PropertyName);
            inputObject.GetProperty<int>(PropertyName).Should().Be(2);
        }

        [TestMethod]
        public void PropertyBagHolder_GetProperty_ThrowsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            Action action = () => inputObject.GetProperty(PropertyName);

            action.ShouldThrow<InvalidOperationException>().WithMessage($"*{PropertyName}*");
        }

        [TestMethod]
        public void PropertyBagHolder_GetPropertyOfT_ThrowsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            Action action = () => inputObject.GetProperty<int>(PropertyName);

            action.ShouldThrow<InvalidOperationException>().WithMessage($"*{PropertyName}*");
        }

        [TestMethod]
        public void PropertyBagHolder_TryGetProperty_ReturnsTrueWhenPropertyExists()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            string value;
            inputObject.TryGetProperty(PropertyName, out value).Should().BeTrue();
            value.Should().Be("x");
        }

        [TestMethod]
        public void PropertyBagHolder_TryGetProperty_ReturnsFalseWhenPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            string value;
            inputObject.TryGetProperty(PropertyName, out value).Should().BeFalse();
            value.Should().BeNull();
        }

        [TestMethod]
        public void PropertyBagHolder_TryGetPropertyOfT_ReturnsTrueWhenPropertyExists()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, 42);

            int value;
            inputObject.TryGetProperty<int>(PropertyName, out value).Should().BeTrue();
            value.Should().Be(42);
        }

        [TestMethod]
        public void PropertyBagHolder_TryGetPropertyOfT_ReturnsFalseWhenPropertyDoesNotExist()
        {
            var inputObject = new TestClass();

            int value;
            inputObject.TryGetProperty<int>(PropertyName, out value).Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public void PropertyBagHolder_RemoveProperty_RemovesExistingProperty()
        {
            var inputObject = new TestClass();
            inputObject.SetProperty(PropertyName, "x");

            inputObject.RemoveProperty(PropertyName);

            inputObject.PropertyNames.Count.Should().Be(0);
            inputObject.ShouldNotContainProperty(PropertyName);
        }

        [TestMethod]
        public void PropertyBagHolder_RemoveProperty_SucceedsIfPropertyDoesNotExist()
        {
            var inputObject = new TestClass();
            inputObject.ShouldNotContainProperty(PropertyName);

            inputObject.RemoveProperty(PropertyName);
            inputObject.ShouldNotContainProperty(PropertyName);
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsStringProperty()
        {
            "def".ShouldSerializeAs("\"def\"");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsIntegerProperty()
        {
            42.ShouldSerializeAs("42");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsLongProperty()
        {
            42L.ShouldSerializeAs("42");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsBooleanProperty()
        {
            true.ShouldSerializeAs("true");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsIntegerArrayProperty()
        {
            new int[] { 5, 12, 13 }.ShouldSerializeAs("[5,12,13]");
        }

        // This demonstrates how the "tags" property is handled.
        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsStringArrayProperty()
        {
            new string[] { "ab", "cde" }.ShouldSerializeAs("[\"ab\",\"cde\"]");
        }

        // These tests show that any enumerable serializes as an array.
        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsStringArrayPropertyFromList()
        {
            new List<string> { "ab", "cde" }.ShouldSerializeAs("[\"ab\",\"cde\"]");
        }

        [TestMethod]
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

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsObjectProperty()
        {
            new TestObjectClass(42, "abc").ShouldSerializeAs("{\"n\":42,\"s\":\"abc\"}");
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsGuidProperty()
        {
            const string GuidString = "{12345678-1234-1234-1234-1234567890ab}";

            // Serializing will strip the braces. We could change SetProperty to
            // special-case Guid and write it out with braces, but there's no
            // point.
            string expectedOutput = '"' + GuidString.Substring(1, GuidString.Length - 2) + '"';

            new Guid(GuidString).ShouldSerializeAs(expectedOutput);
        }

        [TestMethod]
        public void PropertyBagHolder_SetProperty_SetsDateTimeProperty()
        {
            new DateTime(2016, 5, 11, 14, 28, 36, 123).ShouldSerializeAs("\"2016-05-11T14:28:36.123Z\"");
        }
    }

    internal static class ExtensionsForPropertyBagHolderTests
    {
        const string PropertyName = "prop";
        const string Input = "{\"properties\":{\"" + PropertyName + "\":12}}";

        internal static void ShouldSerializeAs<T>(this T value, string serializedValue)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string expectedOutput = "{\"properties\":{\"" + PropertyName + "\":" + serializedValue + "}}";
            var inputObject = JsonConvert.DeserializeObject<TestClass>(Input);
            inputObject.GetProperty<long>(PropertyName).Should().Be(12);

            inputObject.SetProperty(PropertyName, value);

            var serializedObject = JsonConvert.SerializeObject(inputObject);
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
