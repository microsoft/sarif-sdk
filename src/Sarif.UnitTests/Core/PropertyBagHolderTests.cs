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

        public class TestObjectClass
        {
            public TestObjectClass(int n, string s)
            {
                N = n;
                S = s;
            }

            public int N { get; }
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
            const string GuidString = "{12345678-1234-1234-1234-123456780abc}";

            // Serializing will strip the braces. We could change SetProperty to
            // special-case Guid and write it out with braces, but there's no
            // point.
            string expectedOutput = '"' + GuidString.Substring(1, GuidString.Length - 2) + '"';

            new Guid(GuidString).ShouldSerializeAs(expectedOutput);
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
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string expectedOutput = "{\"properties\":{\"" + PropertyName + "\":" + serializedValue + "}}";
            var inputObject = JsonConvert.DeserializeObject<TestClass>(Input);
            inputObject.GetProperty<long>(PropertyName).Should().Be(12);

            inputObject.SetProperty(PropertyName, value);

            var serializedObject = JsonConvert.SerializeObject(inputObject);
            serializedObject.Should().Be(expectedOutput);
        }
    }
}
