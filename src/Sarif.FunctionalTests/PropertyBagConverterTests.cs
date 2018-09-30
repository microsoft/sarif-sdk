// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.FunctionalTests
{
    public class PropertyBagConverterTests
    {
        [Fact]
        [Trait(TestTraits.Bug, "1045")]
        public void PropertyBagConverter_RoundTripsStringPropertyWithEscapedCharacters()
        {
            string originalLog =
@"{
  ""version"": """ + VersionConstants.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner""
      },
      ""properties"": {
        ""int"": 42,
        ""string"": ""'\""\\'""
      }
    }
  ]
}";
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            SarifLog deserializedLog = JsonConvert.DeserializeObject<SarifLog>(originalLog);
            Run run = deserializedLog.Runs[0];

            int integerProperty = run.GetProperty<int>("int");
            integerProperty.Should().Be(42);
            string stringProperty = run.GetProperty<string>("string");
            stringProperty.Should().Be("'\"\\'");

            string reserializedLog = JsonConvert.SerializeObject(deserializedLog, settings);

            reserializedLog.Should().Be(originalLog);
        }
    }
}
