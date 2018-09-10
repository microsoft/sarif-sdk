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
  ""version"": """ + SarifUtilities.SarifFormatVersion + @""",
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
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Formatting.Indented
            };

            SarifLog deserializedLog = JsonConvert.DeserializeObject<SarifLog>(originalLog, serializerSettings);
            Run run = deserializedLog.Runs[0];

            int integerProperty = run.GetProperty<int>("int");
            integerProperty.Should().Be(42);
            string stringProperty = run.GetProperty<string>("string");
            stringProperty.Should().Be("'\"\\'");

            string reserializedLog = JsonConvert.SerializeObject(deserializedLog, serializerSettings);

            reserializedLog.Should().Be(originalLog);
        }
    }
}
