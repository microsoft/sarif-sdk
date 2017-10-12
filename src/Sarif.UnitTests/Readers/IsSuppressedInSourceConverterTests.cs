// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class InSourceSuppressionConverterTests : JsonTests
    {
        [Fact]
        public void SuppressionStatus_SuppressedInSource()
        {
            string expected =
@"{
  ""$schema"": """ + SarifSchemaUri + @""",
  ""version"": """ + SarifFormatVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {
          ""suppressionStates"": [""suppressedInSource""]
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run();

                uut.Initialize(id: null, automationId: null);

                uut.WriteTool(DefaultTool);

                uut.WriteResults(new[] { new Result
                    {
                        SuppressionStates = SuppressionStates.SuppressedInSource
                    }
                });
            });
            actual.Should().BeCrossPlatformEquivalent(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionStates.SuppressedInSource, sarifLog.Runs[0].Results[0].SuppressionStates);
        }

        [Fact]
        public void BaselineState_None()
        {
            string expected =
@"{
  ""$schema"": """ + SarifSchemaUri + @""",
  ""version"": """ + SarifFormatVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {}
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run();
                uut.Initialize(id: null, automationId: null);
                uut.WriteTool(DefaultTool);

                uut.WriteResults(new[] { new Result
                    {
                        BaselineState = BaselineState.None
                    }
                });
            });

            actual.Should().BeCrossPlatformEquivalent(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionStates.None, sarifLog.Runs[0].Results[0].SuppressionStates);
            Assert.Equal(BaselineState.None, sarifLog.Runs[0].Results[0].BaselineState);
        }

        [Fact]
        public void BaselineState_Existing()
        {
            string expected =
@"{
  ""$schema"": """ + SarifSchemaUri + @""",
  ""version"": """ + SarifFormatVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {
          ""baselineState"": ""existing""
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run();

                uut.Initialize(id: null, automationId: null);

                uut.WriteTool(DefaultTool);

                uut.WriteResults(new[] { new Result
                    {
                        BaselineState = BaselineState.Existing
                    }
                });
            });
            actual.Should().BeCrossPlatformEquivalent(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionStates.None, sarifLog.Runs[0].Results[0].SuppressionStates);
            Assert.Equal(BaselineState.Existing, sarifLog.Runs[0].Results[0].BaselineState);
        }
    }
}
