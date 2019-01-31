// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
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
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""DefaultTool""
      },
      ""columnKind"": ""utf16CodeUnits"",
      ""results"": [
          {
            ""message"": {
              ""text"": ""Some testing occurred.""
          },
          ""suppressionStates"": [""suppressedInSource""]
       }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };

                uut.Initialize(run);

                uut.WriteResults(new[] { new Result
                    {
                        Message = new Message { Text = "Some testing occurred."},
                        SuppressionStates = SuppressionStates.SuppressedInSource
                    }
                });
            });
            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionStates.SuppressedInSource, sarifLog.Runs[0].Results[0].SuppressionStates);
        }

        [Fact]
        public void BaselineState_None()
        {
            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""DefaultTool""
      },
      ""columnKind"": ""utf16CodeUnits"",
      ""results"": [
        {
          ""message"": {
            ""text"": ""Some testing occurred.""
        }
       }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                uut.WriteResults(new[] { new Result
                    {
                        Message = new Message { Text = "Some testing occurred."},
                        BaselineState = BaselineState.None
                    }
                });
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionStates.None, sarifLog.Runs[0].Results[0].SuppressionStates);
            Assert.Equal(BaselineState.None, sarifLog.Runs[0].Results[0].BaselineState);
        }

        [Fact]
        public void BaselineState_UnchangedAndUpdated()
        {
            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""DefaultTool""
      },
      ""columnKind"": ""utf16CodeUnits"",
      ""results"": [
        {
          ""message"": {
            ""text"": ""Some testing occurred.""
         },
          ""baselineState"": ""unchanged""
        },
        {
          ""message"": {
            ""text"": ""Some testing occurred.""
         },
          ""baselineState"": ""updated""
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                uut.WriteResults(new[] { new Result
                    {
                        Message = new Message { Text = "Some testing occurred."},
                        BaselineState = BaselineState.Unchanged
                    }
                });
            });
            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionStates.None, sarifLog.Runs[0].Results[0].SuppressionStates);
            Assert.Equal(BaselineState.Unchanged, sarifLog.Runs[0].Results[0].BaselineState);
            Assert.Equal(BaselineState.Updated, sarifLog.Runs[0].Results[0].BaselineState);
        }
    }
}
