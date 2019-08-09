// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif
{
    public class UriConverterTests
    {
        private static readonly ResourceExtractor s_resourceExtractor = new ResourceExtractor(typeof(UriConverterTests));
        private static readonly string s_testFileText = s_resourceExtractor.GetResourceText("JsonConverters.UriConverterTests.json");

        [DataContract]
        private class TestClass
        {
            [DataMember(Name = "nonEmptyUri", IsRequired = false, EmitDefaultValue = false)]
            [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
            public Uri NonEmptyUri { get; set; }

            [DataMember(Name = "emptyUri", IsRequired = false, EmitDefaultValue = false)]
            [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
            public Uri EmptyUri { get; set; }

            [DataMember(Name = "emptyUriList", IsRequired = false, EmitDefaultValue = false)]
            [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
            public IList<Uri> EmptyUriList { get; set; }

            [DataMember(Name = "nonEmptyUriList", IsRequired = false, EmitDefaultValue = false)]
            [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
            public IList<Uri> NonEmptyUriList { get; set; }
        }

        [Fact]
        [Trait(TestTraits.Bug, "1632")]
        public void ReadJson_ConvertsStringsToUriObjects()
        {
            TestClass testObject = JsonConvert.DeserializeObject<TestClass>(s_testFileText);

            testObject.NonEmptyUri.OriginalString.Should().Be("https://www.example.com/rules/TST0001.html");
            testObject.EmptyUri.OriginalString.Should().BeEmpty();
            testObject.EmptyUriList.Should().BeEmpty();
            testObject.NonEmptyUriList.Count().Should().Be(2);
            testObject.NonEmptyUriList.Select(uri => uri.OriginalString).Should().ContainInOrder(
                "https://www.example.com/page1",
                "https://www.example.com/page2");
        }

        [Fact]
        [Trait(TestTraits.Bug, "1632")]
        public void WriteJson_ConvertsUriObjectsToStrings()
        {
            var testObject = new TestClass
            {
                EmptyUri = new Uri(string.Empty, UriKind.RelativeOrAbsolute),
                NonEmptyUri = new Uri("https://www.example.com/rules/TST0001.html"),
                EmptyUriList = new List<Uri>(),
                NonEmptyUriList = new List<Uri>
                {
                    new Uri("https://www.example.com/page1"),
                    new Uri("https://www.example.com/page2")
                }
            };

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            };

            JsonConvert.SerializeObject(testObject, settings).Should().Be(s_testFileText);
        }
    }
}
