// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class CodeFlowLocationKindConverterTests : JsonTests
    {
        [Fact]
        public void CodeFlowLocationKind_AllMembers()
        {
            var testTuples = new List<Tuple<CodeFlowLocationKind, string>>();

            foreach (string name in Enum.GetNames(typeof(CodeFlowLocationKind)))
            {
                var kind = (CodeFlowLocationKind)Enum.Parse(typeof(CodeFlowLocationKind), name);

                if (kind == 0) { continue; }

                string serializedValue = EnumConverter.ConvertToCamelCase(name);
                testTuples.Add(new Tuple<CodeFlowLocationKind, string>(kind, serializedValue));
            }

            foreach (var testTuple in testTuples)
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
          ""codeFlows"": [
            {
              ""locations"": [
                {
                  ""location"": {
                    ""physicalLocation"": {
                      ""fileLocation"": {
                        ""uri"": ""file:///c:/test.c""
                      }
                    }
                  },
                  ""kind"": """ +  testTuple.Item2 + @"""
                }
              ]
            }
          ]
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

                    var result = new Result
                    {
                        CodeFlows = new CodeFlow[]
                        {
                            new CodeFlow
                            {
                                Locations = new CodeFlowLocation[]
                                {
                                    new CodeFlowLocation
                                    {
                                        Location = new Location
                                        {
                                            PhysicalLocation = new PhysicalLocation
                                            {
                                                FileLocation = new FileLocation
                                                {
                                                    Uri = new Uri(@"c:\test.c", UriKind.Absolute)
                                                }
                                            }
                                        },
                                        Kind = testTuple.Item1
                                    }
                                }
                            }
                        }
                    };

                    uut.WriteResults(new[] { result });
                });
                actual.Should().BeCrossPlatformEquivalent(expected);

                var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
                Assert.Equal(testTuple.Item1, sarifLog.Runs[0].Results[0].CodeFlows[0].Locations[0].Kind);
            }
        }
    }
}
