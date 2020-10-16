// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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

            [DataMember(Name = "fileSchemeUri", IsRequired = false, EmitDefaultValue = false)]
            [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
            public Uri FileSchemeUri { get; set; }
        }

        private class SingleUri
        {
            [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
            public Uri Uri { get; set; }
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1632")]
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
            testObject.FileSchemeUri.OriginalString.Should().Be("file:///C:/test/file.c");
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1632")]
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
                },
                FileSchemeUri = new Uri(@"C:\test\file.c")
            };

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            };

            JsonConvert.SerializeObject(testObject, settings).Should().Be(s_testFileText);
        }

        [Fact]
        public void Uri_RoundTripping_Direct()
        {
            // Framework Bug: Uris with certain escaped unreserved characters are doubled by Uri.TryCreate
            // https://github.com/dotnet/runtime/issues/36288

            StringBuilder errors = new StringBuilder();

            SarifUriRoundTrip("http://github.com/Microsoft/sarif-sdk", errors);
            SarifUriRoundTrip("src/Program.cs", errors);
            SarifUriRoundTrip("file:/src/Program.cs", errors);
            SarifUriRoundTrip("file:/../../src/Program.cs", errors);
            SarifUriRoundTrip("file://Code/src/Program.cs", errors);
            SarifUriRoundTrip("file:///C:/Code/src/sarif-sdk/Program.cs", errors);
            SarifUriRoundTrip("file:///new%2Dhost/share/folder/file.cs", errors);
            SarifUriRoundTrip("file:///new%5Fhost/share/folder/file.cs", errors);
            SarifUriRoundTrip("file:///new-host/share/folder%20with%20spaces/file.cs", errors);
            SarifUriRoundTrip("file:///new-host/share/folder/%28surrounding-parens%29.cs", errors);
            SarifUriRoundTrip("file:///%28new-host%29/share/folder/file.cs", errors);
            SarifUriRoundTrip("file:///Local.wiki/WIKI/Engineering/Running%2DTests.md", errors);
            SarifUriRoundTrip("http://www.example.com/dir/file.c", errors);
            SarifUriRoundTrip("http://www.example.com/dir/file name.c", errors);
            SarifUriRoundTrip("http://www.example.com/dir/file%20name.c", errors);
            SarifUriRoundTrip(@"C:\dir\file.c", errors);
            SarifUriRoundTrip(@"C:\dir\file name.c", errors);
            SarifUriRoundTrip("/dir/file.c", errors);
            SarifUriRoundTrip("/dir/file name.c", errors);
            SarifUriRoundTrip("/dir/file%20name.c", errors);
            SarifUriRoundTrip("file:///C:/dir/file.c", errors);
            SarifUriRoundTrip("file:///C:/dir/file name.c", errors);
            SarifUriRoundTrip("file:///C:/dir/file%20name.c", errors);
            SarifUriRoundTrip(@"dir\file.c", errors);
            SarifUriRoundTrip("dir/file.c", errors);
            SarifUriRoundTrip(@"dir\file name.c", errors);
            SarifUriRoundTrip("dir/file%20name.c", errors);
            SarifUriRoundTrip("dir/file name.c", errors);
            SarifUriRoundTrip(@"..\..\.\.\..\dir1\dir2\file.c", errors);
            SarifUriRoundTrip("../../../dir1/dir2/file.c", errors);
            SarifUriRoundTrip(@"..\..", errors);
            SarifUriRoundTrip("../..", errors);

            errors.ToString().Should().BeEmpty();
        }

        private void SarifUriRoundTrip(string value, StringBuilder errors)
        {
            // Put in a class with a Uri using the Sarif 'UriConverter'
            SingleUri sample = new SingleUri();
            sample.Uri = new Uri(value, UriKind.RelativeOrAbsolute);

            // Serialize and Deserialize
            string json = JsonConvert.SerializeObject(sample);
            SingleUri roundTripped = JsonConvert.DeserializeObject<SingleUri>(json);

            if (!roundTripped.Uri.Equals(sample.Uri)) { errors.AppendLine(value); }
        }

        private void DirectUriRoundTrip(string value, StringBuilder errors)
        {
            // .NET can return the string used to construct.
            // This seems like the safest way to roundtrip reliably
            Uri original = new Uri(value, UriKind.RelativeOrAbsolute);
            string serialized = original.OriginalString;
            Uri result = new Uri(serialized, UriKind.RelativeOrAbsolute);

            if (!result.Equals(original)) { errors.AppendLine(value); }
        }
    }
}
