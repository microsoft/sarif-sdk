// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class EnumConverterTests
    {
        [Fact]
        public void EnumConverter_ConvertToPascalCase()
        {
            Assert.Equal("M", EnumConverter.ConvertToPascalCase("m"));
            Assert.Equal("MD", EnumConverter.ConvertToPascalCase("md"));
            Assert.Equal("MD5", EnumConverter.ConvertToPascalCase("md5"));
            Assert.Equal("MDExample", EnumConverter.ConvertToPascalCase("mdExample"));
            Assert.Equal("Mexample", EnumConverter.ConvertToPascalCase("mexample"));

            // NOTE: our heuristics for identifying two letter terms that
            // require casing as a group are necessarily limited to our 
            // specific cases. Doing a reasonable job of this requires 
            // maintaining a dictionary of two letter words to distinguish
            // them from two letter acronyms (which are cased as a group).
            // Even with a dictionary, there is overlap, such as with 
            // Io, a moon of jupiter, and IO.

            Assert.Equal("METoo", EnumConverter.ConvertToPascalCase("meToo"));
        }

        [Fact]
        public void EnumConverter_ConvertToCamelCase()
        {
            Assert.Equal("m", EnumConverter.ConvertToCamelCase("M"));
            Assert.Equal("md", EnumConverter.ConvertToCamelCase("MD"));
            Assert.Equal("md5", EnumConverter.ConvertToCamelCase("MD5"));
            Assert.Equal("mdExample", EnumConverter.ConvertToCamelCase("MDExample"));
            Assert.Equal("mexample", EnumConverter.ConvertToCamelCase("Mexample"));

            // NOTE: our heuristics for identifying two letter terms that
            // require casing as a group are necessarily limited to our 
            // specific cases. Doing a reasonable job of this requires 
            // maintaining a dictionary of two letter words to distinguish
            // them from two letter acronyms (which are cased as a group).
            // Even with a dictionary, there is overlap, such as with 
            // Io, a moon of jupiter, and IO.

            Assert.Equal("meToo", EnumConverter.ConvertToCamelCase("METoo"));
            Assert.Equal("meToo", EnumConverter.ConvertToCamelCase("MeToo"));
        }

        // The following tests pin writer-agnostic round-trip behavior for the
        // converters: JToken round-trip must preserve the value, and text output
        // must remain the SARIF wire shape.

        [Fact]
        public void EnumConverter_RoundTripsThroughJTokenWriter()
        {
            var run = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                ColumnKind = ColumnKind.Utf16CodeUnits,
            };

            JToken token = JToken.FromObject(run, JsonSerializer.CreateDefault());

            JToken columnKindToken = token["columnKind"];
            columnKindToken.Type.Should().Be(JTokenType.String);
            columnKindToken.Value<string>().Should().Be("utf16CodeUnits");

            Run roundTripped = token.ToObject<Run>(JsonSerializer.CreateDefault());
            roundTripped.ColumnKind.Should().Be(ColumnKind.Utf16CodeUnits);
        }

        [Fact]
        public void EnumConverter_TextSerializationOutputIsUnchanged()
        {
            var run = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                ColumnKind = ColumnKind.UnicodeCodePoints,
            };

            string json = JsonConvert.SerializeObject(run);
            json.Should().Contain("\"columnKind\":\"unicodeCodePoints\"");
        }

        [Fact]
        public void FlagsEnumConverter_RoundTripsThroughJTokenWriter()
        {
            var artifact = new Artifact { Roles = ArtifactRoles.AnalysisTarget | ArtifactRoles.Attachment };

            JToken token = JToken.FromObject(artifact, JsonSerializer.CreateDefault());

            JToken rolesToken = token["roles"];
            rolesToken.Type.Should().Be(JTokenType.Array);
            rolesToken.Should().HaveCount(2);
            ((string)rolesToken[0]).Should().Be("analysisTarget");
            ((string)rolesToken[1]).Should().Be("attachment");

            Artifact roundTripped = token.ToObject<Artifact>(JsonSerializer.CreateDefault());
            roundTripped.Roles.Should().Be(ArtifactRoles.AnalysisTarget | ArtifactRoles.Attachment);
        }

        [Fact]
        public void FlagsEnumConverter_TextSerializationOutputIsUnchanged()
        {
            var artifact = new Artifact { Roles = ArtifactRoles.AnalysisTarget | ArtifactRoles.Attachment };
            string json = JsonConvert.SerializeObject(artifact);
            json.Should().Contain("\"roles\":[\"analysisTarget\",\"attachment\"]");
        }

        [Fact]
        public void DateTimeConverter_RoundTripsThroughJTokenWriter()
        {
            var when = new DateTime(2026, 5, 21, 19, 56, 46, 144, DateTimeKind.Utc);
            var invocation = new Invocation { StartTimeUtc = when, ExecutionSuccessful = true };

            JToken token = JToken.FromObject(invocation, JsonSerializer.CreateDefault());

            JToken startToken = token["startTimeUtc"];
            startToken.Type.Should().Be(JTokenType.String);
            startToken.Value<string>().Should().Be("2026-05-21T19:56:46.144Z");

            Invocation roundTripped = token.ToObject<Invocation>(JsonSerializer.CreateDefault());
            roundTripped.StartTimeUtc.Should().Be(when);
        }

        [Fact]
        public void SarifVersionConverter_RoundTripsThroughJTokenWriter()
        {
            var log = new SarifLog { SchemaUri = new Uri("http://json.schemastore.org/sarif-2.1.0-rtm.5"), Version = SarifVersion.Current };

            JToken token = JToken.FromObject(log, JsonSerializer.CreateDefault());

            JToken versionToken = token["version"];
            versionToken.Type.Should().Be(JTokenType.String);

            SarifLog roundTripped = token.ToObject<SarifLog>(JsonSerializer.CreateDefault());
            roundTripped.Version.Should().Be(SarifVersion.Current);
        }
    }
}
